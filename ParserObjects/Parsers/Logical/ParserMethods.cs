namespace ParserObjects.Parsers.Logical
{
    public static class ParserMethods
    {
        public static IParser<TInput, bool> And<TInput>(IParser<TInput> p1, IParser<TInput> p2)
            => new AndParser<TInput>(p1, p2);

        public static IParser<TInput, bool> Not<TInput>(IParser<TInput> p1)
            => new NotParser<TInput>(p1);

        public static IParser<TInput, bool> Or<TInput>(IParser<TInput> p1, IParser<TInput> p2)
            => new OrParser<TInput>(p1, p2);
    }
}