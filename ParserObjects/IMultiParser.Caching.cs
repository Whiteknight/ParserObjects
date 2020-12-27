using ParserObjects.Parsers;

namespace ParserObjects
{
    public static class MultiParserCachingExtensions
    {
        public static IMultiParser<TInput, TOutput> Cache<TInput, TOutput>(this IMultiParser<TInput, TOutput> p)
            => new Cache.MultiParser<TInput, TOutput>(p);
    }
}
