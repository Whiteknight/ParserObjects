namespace ParserObjects.Parsers.Logical
{
    public static class ParserMethods
    {
        public static IParser<TInput> And<TInput>(params IParser<TInput>[] parsers)
            => new AndParser<TInput>(parsers);

        /// <summary>
        /// Invokes the parser if the predicate is satisifed
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> If<TInput, TOutput>(IParser<TInput> predicate, IParser<TInput, TOutput> parser)
            => new IfParser<TInput, TOutput>(predicate, parser);

        public static IParser<TInput> Not<TInput>(IParser<TInput> p1)
            => new NotParser<TInput>(p1);

        public static IParser<TInput> Or<TInput>(params IParser<TInput>[] parsers)
            => new OrParser<TInput>(parsers);
    }
}