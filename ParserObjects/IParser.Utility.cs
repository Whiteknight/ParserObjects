using ParserObjects.Sequences;
using ParserObjects.Visitors;

namespace ParserObjects
{
    public static class ParserExtensions
    {
        /// <summary>
        /// Specify a name for the parser with function syntax.
        /// </summary>
        /// <typeparam name="TParser"></typeparam>
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
        public static ISequence<IResult<TOutput>> ToSequence<TInput, TOutput>(this IParser<TInput, TOutput> parser, ISequence<TInput> input)
            => new ParseResultSequence<TInput, TOutput>(input, parser);
    }
}