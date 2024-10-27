using System;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Double-quoted string literal, with backslash-escaped quotes. The returned string is the string
    /// literal with quotes and escapes.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> DoubleQuotedString()
        => GetOrCreate(
            "Double-Quoted String",
            static () => DelimitedStringWithEscapedDelimiters('"', '"', '\\')
        );

    /// <summary>
    /// Single-quoted string literal, with backslash-escaped quotes. The returned string is the string
    /// literal with quotes and escapes.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> SingleQuotedString()
        => GetOrCreate(
            "Single-Quoted String",
            static () => DelimitedStringWithEscapedDelimiters('\'', '\'', '\\')
        );

    /// <summary>
    /// A parser for delimited strings. Returns the string literal with open sequence, close sequence,
    /// and internal escape sequences.
    /// </summary>
    /// <param name="openStr"></param>
    /// <param name="closeStr"></param>
    /// <param name="escapeStr"></param>
    /// <returns></returns>
    public static IParser<char, string> DelimitedStringWithEscapedDelimiters(char openStr, char closeStr, char escapeStr)
        => Capture(
            Match(openStr),
            Or(
                MatchChars($"{escapeStr}{closeStr}"),
                MatchChars($"{escapeStr}{escapeStr}"),
                Match(c => c != closeStr)
            ).List(),
            Match(closeStr)
        )
        .Stringify();

    /// <summary>
    /// Double-quoted string with backslash-escaped quotes. The returned string is the string without
    /// quotes and without internal escape sequences.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> StrippedDoubleQuotedString()
        => GetOrCreate(
            "Stripped Double-Quoted String",
            static () => StrippedDelimitedStringWithEscapedDelimiters('"', '"', '\\')
        );

    /// <summary>
    /// Single-quoted string with backslash-escaped quotes. The returned string is the string without
    /// quotes and without internal escape sequences.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> StrippedSingleQuotedString()
        => GetOrCreate(
            "Stripped Single-Quoted String",
            static () => StrippedDelimitedStringWithEscapedDelimiters('\'', '\'', '\\').Named("Stripped Single-Quoted String")
        );

    /// <summary>
    /// A parser for delimited strings. Returns the string literal, stripped of open sequence, close
    /// sequence, and internal escape sequences.
    /// </summary>
    /// <param name="openStr"></param>
    /// <param name="closeStr"></param>
    /// <param name="escapeStr"></param>
    /// <returns></returns>
    public static IParser<char, string> StrippedDelimitedStringWithEscapedDelimiters(char openStr, char closeStr, char escapeStr)
        => Rule(
            Match(openStr),
            First(
                MatchChars($"{escapeStr}{closeStr}").Transform(_ => closeStr),
                MatchChars($"{escapeStr}{escapeStr}").Transform(_ => escapeStr),
                Match(c => c != closeStr)
            ).ListCharToString(),
            Match(closeStr),
            static (_, body, _) => body
        );
}
