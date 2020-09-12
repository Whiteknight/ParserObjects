using System;
using System.Collections.Generic;
using ParserObjects.Parsers.Visitors;
using ParserObjects.Sequences;
using ParserObjects.Utility;

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

    /// <summary>
    /// A parser which has an in-place replaceable child. Used to identify parsers which can participate in
    /// certain find/replace operations
    /// </summary>
    public interface IReplaceableParserUntyped : IParser
    {
        /// <summary>
        /// The child parser which can be replaced without cloning
        /// </summary>
        IParser ReplaceableChild { get; }

        /// <summary>
        /// Set the new child parser without cloning
        /// </summary>
        /// <param name="parser"></param>
        SingleReplaceResult SetParser(IParser parser);
    }

    /// <summary>
    /// General-purpose extensions for IParser and descendents
    /// </summary>
    public static class ParserExtensions
    {
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
        /// consume any input. Returns true if the parse would succeed, false otherwise.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanMatch<TOutput>(this IParser<char, TOutput> parser, string input)
            => CanMatch(parser, new StringCharacterSequence(input));

        /// <summary>
        /// Recurse the tree searching for a parser with the given name. Returns the first matching result.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IParser FindNamed(this IParser root, string name) => FindParserVisitor.Named(name, root);

        /// <summary>
        /// Specify a name for the parser with function syntax.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TParser Named<TParser>(this TParser parser, string name)
             where TParser : IParser
        {
            parser.Name = name;
            return parser;
        }

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

        /// <summary>
        /// Given a parser tree, replace all children of ReplaceableParsers matching the given predicate with
        /// the provided replacement parser.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="predicate"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace(this IParser root, Func<IParser, bool> predicate, IParser replacement)
            => FindParserVisitor.Replace(root, predicate, replacement);

        /// <summary>
        /// Given a parser tree, find a ReplaceableParser with a child which is reference equal to the given
        /// find parser, and replaces it with the given replacement parser.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="find"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace(this IParser root, IParser find, IParser replacement)
            => Replace(root, p => ReferenceEquals((p as IReplaceableParserUntyped)?.ReplaceableChild, find), replacement);

        /// <summary>
        /// Given a parser tree, find a ReplaceableParsers matching a predicate and attempt to transform
        /// the contents using the given transformation. The contents of the ReplaceableParser will be
        /// replaced with the transformed result if it is new and valid.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="root"></param>
        /// <param name="predicate"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace<TInput, TOutput>(this IParser root, Func<IParser, bool> predicate, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> transform)
            => FindParserVisitor.Replace(root, predicate, transform);

        /// <summary>
        /// Given a parser tree, find a ReplaceableParser with the given name and replace it's child parser
        /// with the given replacement parser
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace(this IParser root, string name, IParser replacement)
            => FindParserVisitor.Replace(root, name, replacement);

        /// <summary>
        /// Given a parser tree, find a ReplaceableParser matching a predicate and attempt to transform
        /// the contents using the given transformation. The contents of the ReplaceableParser will be
        /// replaced with the transformed result if it is new and valid.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace<TInput, TOutput>(this IParser root, string name, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> transform)
            => FindParserVisitor.Replace(root, name, transform);

        /// <summary>
        /// Attempt to describe the parser as a string of pseudo-BNF. This feature depends on parsers having
        /// a .Name value set. If you are using custom IParser implementations you will need to use a custom
        /// BnfStringifyVisitor subclass to account for it.
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static string ToBnf(this IParser parser)
            => new BnfStringifyVisitor().ToBnf(parser);

        /// <summary>
        /// Convert a parser and it's input sequence into a new sequence of parse result values
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static ISequence<IParseResult<TOutput>> ToSequence<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input)
            => new ParseResultSequence<TInput, TOutput>(input, parser);
    }
}