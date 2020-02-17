namespace ParserObjects.Parsers.Logical
{
    public static class ParserExtensions
    {
        public static IParser<TInput, bool> And<TInput>(this IParser<TInput> p1, IParser<TInput> p2)
            => new AndParser<TInput>(p1, p2);

        public static IParser<TInput, bool> Not<TInput>(this IParser<TInput> p1)
            => new NotParser<TInput>(p1);

        public static IParser<TInput, bool> Or<TInput>(this IParser<TInput> p1, IParser<TInput> p2)
            => new OrParser<TInput>(p1, p2);
    }
}