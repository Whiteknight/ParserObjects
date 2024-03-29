﻿using System;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Double-quoted string literal, with backslash-escaped quotes. The returned string is the string
    /// literal with quotes and escapes.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> DoubleQuotedString() => _doubleQuotedString.Value;

    private static readonly Lazy<IParser<char, string>> _doubleQuotedString = new Lazy<IParser<char, string>>(
        static () => DelimitedStringWithEscapedDelimiters('"', '"', '\\').Named("Double-Quoted String")
    );

    /// <summary>
    /// Single-quoted string literal, with backslash-escaped quotes. The returned string is the string
    /// literal with quotes and escapes.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> SingleQuotedString() => _singleQuotedString.Value;

    private static readonly Lazy<IParser<char, string>> _singleQuotedString = new Lazy<IParser<char, string>>(
        static () => DelimitedStringWithEscapedDelimiters('\'', '\'', '\\').Named("Single-Quoted String")
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
    {
        var escapedClose = $"{escapeStr}{closeStr}";
        var escapedEscape = $"{escapeStr}{escapeStr}";
        var bodyChar = Or(
            MatchChars(escapedClose),
            MatchChars(escapedEscape),
            Match(c => c != closeStr)
        );
        return Capture(
                Match(openStr),
                bodyChar.List(),
                Match(closeStr)
            )
            .Stringify();
    }

    /// <summary>
    /// Double-quoted string with backslash-escaped quotes. The returned string is the string without
    /// quotes and without internal escape sequences.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> StrippedDoubleQuotedString() => _strippedDoubleQuotedString.Value;

    private static readonly Lazy<IParser<char, string>> _strippedDoubleQuotedString = new Lazy<IParser<char, string>>(
        static () => StrippedDelimitedStringWithEscapedDelimiters('"', '"', '\\').Named("Stripped Double-Quoted String")
    );

    /// <summary>
    /// Single-quoted string with backslash-escaped quotes. The returned string is the string without
    /// quotes and without internal escape sequences.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> StrippedSingleQuotedString() => _strippedSingleQuotedString.Value;

    private static readonly Lazy<IParser<char, string>> _strippedSingleQuotedString = new Lazy<IParser<char, string>>(
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
    {
        var escapedClose = $"{escapeStr}{closeStr}";
        var escapedEscape = $"{escapeStr}{escapeStr}";
        var bodyChar = First(
            MatchChars(escapedClose).Transform(_ => closeStr),
            MatchChars(escapedEscape).Transform(_ => escapeStr),
            Match(c => c != closeStr)
        );
        return Rule(
            Match(openStr),
            bodyChar.ListCharToString(),
            Match(closeStr),
            static (_, body, _) => body
        );
    }
}
