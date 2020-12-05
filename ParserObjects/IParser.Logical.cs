using System.Linq;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static class ParserLogicalExtensions
    {
        /// <summary>
        /// Parse the given parser and all additional parsers sequentially. Consumes input but returns no
        /// output. Will probably be used by Positive- or Negative-lookahead or If.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput> And<TInput>(this IParser<TInput> p1, params IParser<TInput>[] parsers)
            => new AndParser<TInput>(new[] { p1 }.Concat(parsers).ToArray());

        /// <summary>
        /// Attempt to parse with a predicate parser, consuming no input. If the predicate parser succeeds,
        /// parse with the given parser.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> If<TInput, TOutput>(this IParser<TInput, TOutput> parser, IParser<TInput> predicate)
            => new IfParser<TInput, TOutput>(predicate, parser, ParserMethods<TInput>.Fail<TOutput>());

        /// <summary>
        /// Parses with the given parser, inverting the result so Success becomes Failure and Failure becomes
        /// Success. Consumes input but returns no output. Will probably be used by Positive- or
        /// Negative-lookahead or If.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static IParser<TInput> Not<TInput>(this IParser<TInput> p1)
            => new NotParser<TInput>(p1);

        /// <summary>
        /// Attempts to parse with each parser successively, returning Success if any parser succeeds
        /// or Failure if none do. Consumes input but returns no output. Will probably be used by
        /// Positive- or Negative-lookahed or If.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput> Or<TInput>(this IParser<TInput> p1, params IParser<TInput>[] parsers)
            => new OrParser<TInput>(new[] { p1 }.Concat(parsers).ToArray());

        /// <summary>
        /// Attempt to parse with a predicate parser. If the predicate parser succeeds,
        /// parse with the given parser. This is the same operation as If with different order of operands.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Then<TInput, TOutput>(this IParser<TInput> predicate, IParser<TInput, TOutput> parser)
            => new IfParser<TInput, TOutput>(predicate, parser, ParserMethods<TInput>.Fail<TOutput>());
    }
}