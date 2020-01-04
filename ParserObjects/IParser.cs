using System;
using System.Collections.Generic;
using ParserObjects.Parsers.Visitors;
using ParserObjects.Sequences;

namespace ParserObjects
{
    public interface IParser
    {
        string Name { get; set; }
        //IParser Accept(IParserVisitor visitor);
        IEnumerable<IParser> GetChildren();
        IParser ReplaceChild(IParser find, IParser replace);
    }

    public interface IParser<TInput> : IParser
    {
        IParseResult<object> ParseUntyped(ISequence<TInput> t);
    }

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
        public static IParser<TInput, TOutput> Named<TInput, TOutput>(this IParser<TInput, TOutput> parser, string name)
        {
            parser.Name = name;
            return parser;
        }

        public static IParser<TInput, TOutput> Replace<TInput, TOutput>(this IParser<TInput, TOutput> root, Func<IParser, bool> predicate, IParser replacement)
            => new ReplaceParserVisitor(predicate, replacement).Visit(root) as IParser<TInput, TOutput>;

        public static IParser FindNamed(this IParser root, string name) => FindParserVisitor.Named(name, root);

        public static IParser<TInput, TOutput> Replace<TInput, TOutput>(this IParser<TInput, TOutput> root, IParser find, IParser replace)
            => new ReplaceParserVisitor(p => p == find, replace).Visit(root) as IParser<TInput, TOutput>;

        public static bool CanMatch<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input)
        {
            var window = new WindowSequence<TInput>(input);
            var result = parser.Parse(window);
            window.Rewind();
            return result.Success;
        }

        public static bool CanMatch<TOutput>(this IParser<char, TOutput> parser, string input)
        {
            return CanMatch(parser, new StringCharacterSequence(input));
        }
    }
}