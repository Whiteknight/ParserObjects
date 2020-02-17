namespace ParserObjects.Parsers.Logical
{
    public static class ParserExtensions
    {
        public static IParser<TInput, bool> And<TInput>(this IParser<TInput, bool> p1, IParser<TInput, bool> p2)
            => new AndParser<TInput>(p1, p2);

        /// <summary>
        /// Parse if the predicate succeeds
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> If<TInput, TOutput>(this IParser<TInput, TOutput> parser, IParser<TInput, bool> predicate)
            => new IfParser<TInput, TOutput>(predicate, parser);

        public static IParser<TInput, bool> Not<TInput>(this IParser<TInput, bool> p1)
            => new NotParser<TInput>(p1);

        public static IParser<TInput, bool> Or<TInput>(this IParser<TInput, bool> p1, IParser<TInput, bool> p2)
            => new OrParser<TInput>(p1, p2);

        /// <summary>
        /// If the predicate succeeds, invoke the parser
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Then<TInput, TOutput>(this IParser<TInput, bool> predicate, IParser<TInput, TOutput> parser)
            => new IfParser<TInput, TOutput>(predicate, parser);
    }
}