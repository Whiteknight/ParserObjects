using System;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects
{
    public static partial class ParserMethods
    {
        /// <summary>
        /// Double-quoted string literal, with backslash-escaped quotes. The returned string is the string
        /// literal with quotes and escapes
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DoubleQuotedString() => _doubleQuotedString.Value;
        private static readonly Lazy<IParser<char, string>> _doubleQuotedString = new Lazy<IParser<char, string>>(
            () => DelimitedStringWithEscapedDelimiters('"', '"', '\\').Named("Double-Quoted String")
        );

        /// <summary>
        /// Single-quoted string literal, with backslash-escaped quotes. The returned string is the string
        /// literal with quotes and escapes
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> SingleQuotedString() => _singleQuotedString.Value;
        private static readonly Lazy<IParser<char, string>> _singleQuotedString = new Lazy<IParser<char, string>>(
            () => DelimitedStringWithEscapedDelimiters('\'', '\'', '\\').Named("Single-Quoted String")
        );

        /// <summary>
        /// A parser for delimited strings. Returns the string literal with open sequence, close sequence,
        /// and internal escape sequences
        /// </summary>
        /// <param name="openStr"></param>
        /// <param name="closeStr"></param>
        /// <param name="escapeStr"></param>
        /// <returns></returns>
        public static IParser<char, string> DelimitedStringWithEscapedDelimiters(char openStr, char closeStr, char escapeStr)
        {
            var escapedClose = $"{escapeStr}{closeStr}";
            var escapedEscape = $"{escapeStr}{escapeStr}";
            var bodyChar = First(
                Match(escapedClose).Transform(c => escapedClose),
                Match(escapedEscape).Transform(c => escapedEscape),
                Match(c => c != closeStr).Transform(c => c.ToString())
            );
            return Rule(
                Match(openStr).Transform(c => openStr.ToString()),
                bodyChar.ListStringsToString(),
                Match(closeStr).Transform(c => closeStr.ToString()),

                (open, body, close) => open + body + close
            );
        }

        /// <summary>
        /// Double-quoted string with backslash-escaped quotes. The returned string is the string without
        /// quotes and without internal escape sequences
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedDoubleQuotedString() => _strippedDoubleQuotedString.Value;
        private static readonly Lazy<IParser<char, string>> _strippedDoubleQuotedString = new Lazy<IParser<char, string>>(
            () => StrippedDelimitedStringWithEscapedDelimiters('"', '"', '\\').Named("Stripped Double-Quoted String")
        );

        /// <summary>
        /// Single-quoted string with backslash-escaped quotes. The returned string is the string without
        /// quotes and without internal escape sequences
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedSingleQuotedString() => _strippedSingleQuotedString.Value;
        private static readonly Lazy<IParser<char, string>> _strippedSingleQuotedString = new Lazy<IParser<char, string>>(
            () => StrippedDelimitedStringWithEscapedDelimiters('\'', '\'', '\\').Named("Stripped Single-Quoted String")
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
                Match(escapedClose).Transform(s => closeStr),
                Match(escapedEscape).Transform(s => escapeStr),
                Match(c => c != closeStr)
            );
            return Rule(
                Match(openStr),
                bodyChar.ListCharToString(),
                Match(closeStr),

                (open, body, close) => body
            );
        }
    }
}