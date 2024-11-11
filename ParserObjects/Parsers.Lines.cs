using System;
using ParserObjects.Internal.Parsers;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Parser that returns success at end of input and immediately before a newline. Failure
    /// otherwise.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, object> EndOfLine()
        => GetOrCreate(
            "End of Line",
            static () => PositiveLookahead(
                First(
                    MatchChar('\n'),
                    End()
                )
            )
        );

    /// <summary>
    /// Parses a line of text, starting with a prefix and going until a newline or end
    /// of input. Newline not included.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static IParser<char, string> PrefixedLine(string prefix)
        => string.IsNullOrEmpty(prefix)
            ? Line()
            : CaptureString(
                MatchChars(prefix),
                Match(static c => c != '\n').List()
            )
            .Named($"Line Prefixed:{prefix}");

    /// <summary>
    /// Parses a line of text until a newline or end of input. Newline not included.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Line()
        => GetOrCreate(
            "Line",
            static () => Match(static c => c != '\n').ListCharToString()
        );

    private static readonly IParser<char, object> _startOfLine = new SequenceFlagParser<char>(
        SequenceStateTypes.StartOfLine,
        "Expected start of line but found ");

    /// <summary>
    /// Parser that returns true at Start of Input and immediately after a newline. Failure
    /// otherwise.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, object> StartOfLine() => _startOfLine;
}
