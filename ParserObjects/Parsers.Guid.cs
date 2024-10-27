using System;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Guid in the C# 'N' format XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidN()
        => GetOrCreate(
            "Guid(N)",
            static () => CaptureString(HexadecimalDigit().List(32, 32)).Transform(Guid.Parse)
        );

    /// <summary>
    ///  Guid in the C# 'D' format XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidD()
        => GetOrCreate(
            "Guid(D)",
            static () => CaptureString(
                HexadecimalDigit().List(8, 8),
                MatchChar('-'),
                HexadecimalDigit().List(4, 4),
                MatchChar('-'),
                HexadecimalDigit().List(4, 4),
                MatchChar('-'),
                HexadecimalDigit().List(4, 4),
                MatchChar('-'),
                HexadecimalDigit().List(12, 12)
            )
            .Transform(Guid.Parse)
            .Named("Guid(D)")
        );

    /// <summary>
    ///  Guid in the C# 'B' format {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidB()
        => GetOrCreate(
            "Guid(B)",
            static () => Rule(
                MatchChar('{'),
                GuidD(),
                MatchChar('}'),
                (_, g, _) => g
            )
        );

    /// <summary>
    ///  Guid in the C# 'P' format (XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX).
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidP()
        => GetOrCreate(
            "Guid(P)",
            static () => Rule(
                MatchChar('('),
                GuidD(),
                MatchChar(')'),
                (_, g, _) => g
            )
        );
}
