using System;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Guid in the C# 'N' format XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidN() => _guidN.Value;

    private static readonly Lazy<IParser<char, Guid>> _guidN = new Lazy<IParser<char, Guid>>(
        () => CaptureString(HexadecimalDigit().List(32, 32))
            .Transform(Guid.Parse)
            .Named("Guid(N)")
    );

    /// <summary>
    ///  Guid in the C# 'D' format XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidD() => _guidD.Value;

    private static readonly Lazy<IParser<char, Guid>> _guidD = new Lazy<IParser<char, Guid>>(
        () => _guidDInternal!.Value
            .Transform(Guid.Parse)
            .Named("Guid(D)")
    );

    private static readonly Lazy<IParser<char, string>> _guidDInternal = new Lazy<IParser<char, string>>(
        () => CaptureString(
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
    );

    /// <summary>
    ///  Guid in the C# 'B' format {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidB() => _guidB.Value;

    private static readonly Lazy<IParser<char, Guid>> _guidB = new Lazy<IParser<char, Guid>>(
        () => Rule(
                MatchChar('{'),
                _guidDInternal!.Value,
                MatchChar('}'),
                (_, g, _) => g
            )
            .Transform(Guid.Parse)
            .Named("Guid(B)")
    );

    /// <summary>
    ///  Guid in the C# 'P' format (XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX).
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Guid> GuidP() => _guidP.Value;

    private static readonly Lazy<IParser<char, Guid>> _guidP = new Lazy<IParser<char, Guid>>(
        () => Rule(
                MatchChar('('),
                _guidDInternal!.Value,
                MatchChar(')'),
                (_, g, _) => g
            )
            .Transform(Guid.Parse)
            .Named("Guid(P)")
    );
}
