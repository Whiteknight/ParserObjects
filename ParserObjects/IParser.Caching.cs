using ParserObjects.Parsers;

namespace ParserObjects
{
    public static class ParserCachingExtensions
    {
        public static IParser<TInput, TOutput> Cache<TInput, TOutput>(this IParser<TInput, TOutput> p)
            => new Cache.OutputParser<TInput, TOutput>(p);

        public static IParser<TInput> Cache<TInput>(this IParser<TInput> p)
            => new Cache.NoOutputParser<TInput>(p);
    }
}
