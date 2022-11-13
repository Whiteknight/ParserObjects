namespace ParserObjects;

public static class MultiParserCachingExtensions
{
    /// <summary>
    /// Cache result values of this parser so on the next call to Parse from the same location
    /// an existing result value can be used.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Cache<TInput, TOutput>(this IMultiParser<TInput, TOutput> p)
        => Parsers<TInput>.Cache(p);
}
