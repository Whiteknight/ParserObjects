using System;
using ParserObjects.Sequences;
using ParserObjects.Utility;
using ParserObjects.Visitors;

namespace ParserObjects
{
    public static class ParserExtensions
    {
        /// <summary>
        /// Attempt to describe the parser as a string of pseudo-BNF. This feature depends on parsers having
        /// a .Name value set. If you are using custom IParser implementations you will need to use a custom
        /// BnfStringifyVisitor subclass to account for it.
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static string ToBnf(this IParser parser)
        {
            if (string.IsNullOrEmpty(parser.Name))
            {
                var old = parser.Name;
                parser.Name = "(TARGET)";
                var value = new BnfStringifyVisitor().ToBnf(parser);
                parser.Name = old;
                return value;
            }

            return new BnfStringifyVisitor().ToBnf(parser);
        }

        /// <summary>
        /// Convert a parser and it's input sequence into a new sequence of parse result values.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="input"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static ISequence<IResult<TOutput>> ToSequence<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input, Action<string>? log = null)
            => new ParseResultSequence<TInput, TOutput>(input, parser, log ?? Defaults.LogMethod);
    }
}
