using System.Linq;

namespace ParserObjects.Parsers.Logical
{
    public static class ParserExtensions
    {
        public static IParser<TInput> And<TInput>(this IParser<TInput> p1, params IParser<TInput>[] parsers)
            => new AndParser<TInput>(new [] { p1 }.Concat(parsers).ToArray());

        /// <summary>
        /// Parse if the predicate succeeds
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> If<TInput, TOutput>(this IParser<TInput, TOutput> parser, IParser<TInput> predicate)
            => new IfParser<TInput, TOutput>(predicate, parser);

        public static IParser<TInput> Not<TInput>(this IParser<TInput> p1)
            => new NotParser<TInput>(p1);

        public static IParser<TInput> Or<TInput>(this IParser<TInput> p1, params IParser<TInput>[] parsers)
            => new OrParser<TInput>(new[] { p1 }.Concat(parsers).ToArray());

        /// <summary>
        /// If the predicate succeeds, invoke the parser
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Then<TInput, TOutput>(this IParser<TInput> predicate, IParser<TInput, TOutput> parser)
            => new IfParser<TInput, TOutput>(predicate, parser);
    }
}