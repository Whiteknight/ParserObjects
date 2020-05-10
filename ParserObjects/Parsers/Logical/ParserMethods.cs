namespace ParserObjects.Parsers.Logical
{
    public static class ParserMethods<TInput>
    {
        /// <summary>
        /// Tests several parsers sequentially. Returns success if they all succeed, otherwise
        /// returns failure. Consumes input but returns no explicit output.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput> And(params IParser<TInput>[] parsers)
            => new AndParser<TInput>(parsers);

        /// <summary>
        /// Tests the predicate parser, consuming no input. If the predicate succeeds, perform the parse.
        /// Otherwise return Failure.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> If<TOutput>(IParser<TInput> predicate, IParser<TInput, TOutput> parser)
            => new IfParser<TInput, TOutput>(predicate, parser);

        /// <summary>
        /// Invoke the given parser and invert the result. On Success return Failure, on Failure return
        /// Success. Consumes input but returns no output.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static IParser<TInput> Not(IParser<TInput> p1)
            => new NotParser<TInput>(p1);

        /// <summary>
        /// Tests several parsers sequentially. Returns Success if any parser succeeds, returns
        /// Failure otherwise. Consumes input but returns no explicit output.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput> Or(params IParser<TInput>[] parsers)
            => new OrParser<TInput>(parsers);
    }
}