namespace ParserObjects;

public static class ParserCachingExtensions
{
    /// <summary>
    /// Cache the output of the given parser so that the next call to .Parse at the same
    /// location will return the existing result. Useful when the parser is particularly
    /// expensive to execute and likely to be executed multiple times.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Cache<TInput, TOutput>(this IParser<TInput, TOutput> p)
        => Parsers<TInput>.Cache(p);
}
