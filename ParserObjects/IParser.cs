using System;
using System.Collections.Generic;
using ParserObjects.Parsers.Visitors;
using ParserObjects.Sequences;

namespace ParserObjects
{
    /// <summary>
    /// Parser base type.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// The name of the parser. This value is only used for bookkeeping information and does not have
        /// an affect on the parse.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Get a list of child parsers, if any.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IParser> GetChildren();

        /// <summary>
        /// Return a parser exactly like this parser but with one of it's children replaced. If no
        /// replacement is made, this parser may be returned. 
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        IParser ReplaceChild(IParser find, IParser replace);
    }

    /// <summary>
    /// Parser object which allows getting the result without type information
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public interface IParser<TInput> : IParser
    {
        /// <summary>
        /// Attempt to parse the input sequence and produce an output result of type object. If the parse
        /// fails, it is expected that this method will return the input sequence to the state it was at
        /// before the parse was attempted.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        IParseResult<object> ParseUntyped(ISequence<TInput> t);
    }

    /// <summary>
    /// Parser with explicit input and output types.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IParser<TInput, out TOutput> : IParser<TInput>
    {
        /// <summary>
        /// Attempt to parse the input sequence and produce an output result. If the parse fails, it is
        /// expected that this method will return the input sequence to the state it was at before the
        /// parse was attempted.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        IParseResult<TOutput> Parse(ISequence<TInput> t);
    }

    public static class ParserExtensions
    {
        /// <summary>
        /// Specify a name for the parser with function syntax.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Named<TInput, TOutput>(this IParser<TInput, TOutput> parser, string name)
        {
            parser.Name = name;
            return parser;
        }

        /// <summary>
        /// Given a parser tree, replace all parsers which satisfy the predicate with some new parser value.
        /// Returns the updated tree
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="root"></param>
        /// <param name="predicate"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replace<TInput, TOutput>(this IParser<TInput, TOutput> root, Func<IParser, bool> predicate, IParser replacement)
            => new ReplaceParserVisitor(predicate, replacement).Visit(root) as IParser<TInput, TOutput>;

        /// <summary>
        /// Recurse the tree searching for a parser with the given name. Returns the first matching result.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IParser FindNamed(this IParser root, string name) => FindParserVisitor.Named(name, root);

        /// <summary>
        /// Given a parser tree, replace the given parser with the new parser. Returns the updated tree.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="root"></param>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replace<TInput, TOutput>(this IParser<TInput, TOutput> root, IParser find, IParser replace)
            => new ReplaceParserVisitor(p => p == find, replace).Visit(root) as IParser<TInput, TOutput>;

        /// <summary>
        /// Attempts a parse but does not consume any input. Instead it returns a boolean true if the parse
        /// succeeded or false otherwise.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanMatch<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input)
        {
            var window = new WindowSequence<TInput>(input);
            var result = parser.Parse(window);
            window.Rewind();
            return result.Success;
        }

        /// <summary>
        /// Convenience method for parsers which act on character sequences. Attempts a parse but does not
        /// consume any input. Instead it returns a boolean true if the parse succeeds or false otherwise.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanMatch<TOutput>(this IParser<char, TOutput> parser, string input) 
            => CanMatch(parser, new StringCharacterSequence(input));

        /// <summary>
        /// Convenience method for parser which act on character sequences. Parse the given input string
        /// and return the first value or failure.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IParseResult<TOutput> Parse<TOutput>(this IParser<char, TOutput> parser, string s)
            => parser.Parse(new StringCharacterSequence(s));
    }
}