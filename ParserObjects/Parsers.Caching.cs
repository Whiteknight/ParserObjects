using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Cache the result of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput> Cache(IParser<TInput> parser)
        => new Cache<TInput>.Parser(parser);

    /// <summary>
    /// Cache the result of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Cache<TOutput>(IParser<TInput, TOutput> parser)
        => new Cache<TInput>.Parser<TOutput>(parser);

    /// <summary>
    /// Cache the result of the given parser so subsequent calls to .Parse at the same
    /// location will return the same result.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Cache<TOutput>(IMultiParser<TInput, TOutput> parser)
        => new Cache<TInput>.MultiParser<TOutput>(parser);
}
