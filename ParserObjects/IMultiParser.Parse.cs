using ParserObjects.Internal;
using static ParserObjects.Sequences;

namespace ParserObjects;

public static partial class MultiParserExtensions
{
    /// <summary>
    /// Parse a string using the given character multiparser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static MultiResult<TOutput> Parse<TOutput>(this IMultiParser<char, TOutput> p, string s)
        => p.Parse(
            new ParseState<char>(
                FromString(s, default),
                Defaults.LogMethod
            )
        );
}
