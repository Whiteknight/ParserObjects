using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Zero-length assertion that the given pattern does not match from the current position. No
    /// input is consumed.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, object> NegativeLookahead(IParser<TInput> p)
        => new NegativeLookaheadParser<TInput>(p);

    /// <summary>
    /// Zero-length assertion that the given pattern matches from the current position. No input is
    /// consumed.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, object> PositiveLookahead(IParser<TInput> p)
        => new PositiveLookaheadParser<TInput>(p);
}
