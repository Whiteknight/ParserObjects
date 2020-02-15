using System.Globalization;
using ParserObjects.Utility;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Specialty.NumberParserMethods;
using static ParserObjects.Parsers.Specialty.LineParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class ProgrammingParserMethods
    {
        /// <summary>
        /// C-style comment with /* ...  */ delimiters
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> CStyleComment()
            => ParserCache.Instance.GetParser(nameof(CStyleComment), Internal._CStyleComment);

        /// <summary>
        /// C-style hexadecimal literal
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> CStyleHexadecimalLiteral()
            => ParserCache.Instance.GetParser(nameof(CStyleHexadecimalLiteral), Internal._CStyleHexadecimalLiteral);

        /// <summary>
        /// C-style Integer literal
        /// </summary>
        /// <returns></returns>
        public static IParser<char, int> CStyleIntegerLiteral()
            => ParserCache.Instance.GetParser(nameof(CStyleIntegerLiteral), Internal._CStyleIntegerLiteral);

        /// <summary>
        /// C-style float/double literal
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> CStyleDoubleLiteral()
            => ParserCache.Instance.GetParser(nameof(CStyleDoubleLiteral), Internal._CStyleDoubleLiteral);

        /// <summary>
        /// C-style Identifier
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> CStyleIdentifier()
            => ParserCache.Instance.GetParser(nameof(CStyleIdentifier), Internal._CStyleIdentifier);

        /// <summary>
        /// JavaScript-style number literal, returned as a double
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> JavaScriptStyleNumberLiteral()
            => ParserCache.Instance.GetParser(nameof(JavaScriptStyleNumberLiteral), Internal._JavaScriptStyleNumberLiteral);

        /// <summary>
        /// C++-style comment //...
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> CPlusPlusStyleComment()
            => ParserCache.Instance.GetParser(nameof(CPlusPlusStyleComment), Internal._CPlusPlusStyleComment);

        /// <summary>
        /// SQL-style comment --....
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> SqlStyleComment()
            => ParserCache.Instance.GetParser(nameof(SqlStyleComment), Internal._SqlStyleComment);

        /// <summary>
        /// Double-quoted string literal, with backslash-escaped quotes. The returned string is the string
        /// literal with quotes and escapes
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DoubleQuotedStringWithEscapedQuotes()
            => ParserCache.Instance.GetParser(nameof(DoubleQuotedStringWithEscapedQuotes), Internal._DoubleQuotedStringWithEscapedQuotes);

        /// <summary>
        /// Single-quoted string literal, with backslash-escaped quotes. The returned string is the string
        /// literal with quotes and escapes
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> SingleQuotedStringWithEscapedQuotes()
            => ParserCache.Instance.GetParser(nameof(SingleQuotedStringWithEscapedQuotes), Internal._SingleQuotedStringWithEscapedQuotes);

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
            var escapedClose = escapeStr.ToString() + closeStr.ToString();
            var escapedEscape = escapeStr.ToString() + escapeStr.ToString();
            var bodyChar = First(
                Match<char>(escapedClose).Transform(c => escapedClose),
                Match<char>(escapedEscape).Transform(c => escapedEscape),
                Match<char>(c => c != closeStr).Transform(c => c.ToString())
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
        public static IParser<char, string> StrippedDoubleQuotedStringWithEscapedQuotes()
            => ParserCache.Instance.GetParser(nameof(StrippedDoubleQuotedStringWithEscapedQuotes), Internal._StrippedDoubleQuotedStringWithEscapedQuotes);

        /// <summary>
        /// Single-quoted string with backslash-escaped quotes. The returned string is the string without
        /// quotes and without internal escape sequences
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedSingleQuotedStringWithEscapedQuotes()
            => ParserCache.Instance.GetParser(nameof(StrippedSingleQuotedStringWithEscapedQuotes), Internal._StrippedSingleQuotedStringWithEscapedQuotes);

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
            var escapedClose = escapeStr.ToString() + closeStr.ToString();
            var escapedEscape = escapeStr.ToString() + escapeStr.ToString();
            var bodyChar = First(
                Match<char>(escapedClose).Transform(s => closeStr),
                Match<char>(escapedEscape).Transform(s => escapeStr),
                Match<char>(c => c != closeStr)
            );
            return Rule(
                Match(openStr),
                bodyChar.ListCharToString(),
                Match(closeStr),

                (open, body, close) => body
            );
        }

        // TODO: C-style string literals with escapes
        // TODO: C-style char literals with escapes
        // TODO: JS-style string literals with escapes (https://mathiasbynens.be/notes/javascript-escapes)

        private static class Internal
        {
            public static IParser<char, string> _CStyleComment()
            {
                var start = Match<char>("/*").Transform(c => "/*");
                var end = Match<char>("*/").Transform(c => "*/");

                var bodyChar = First(
                    Rule(
                        Match('*'),
                        NegativeLookahead(Match('/')),
                        (star, slash) => star
                    ),
                    Match<char>(c => c != '*')
                );

                return Rule(
                    start,
                    bodyChar.ListCharToString(),
                    end,
                    (s, b, e) => s + b + e
                );
            }

            // TODO: Variants of all these methods which return the number string
            public static IParser<char, int> _CStyleHexadecimalLiteral()
            {
                return Rule(
                    Match<char>("0x"),
                    HexadecimalString(),

                    (prefix, value) => int.Parse(value, NumberStyles.HexNumber)
                );
            }

            public static IParser<char, int> _CStyleIntegerLiteral()
            {
                return Rule(
                    Match('-').Transform(c => "-").Optional(() => string.Empty),
                    Digit().ListCharToString(true),

                    (sign, value) => int.Parse(sign + value)
                );
            }

            public static IParser<char, double> _CStyleDoubleLiteral()
            {
                return Rule(
                    Match('-').Transform(c => "-").Optional(() => ""),
                    DigitString(),
                    Match('.').Transform(c => "."),
                    DigitString(),

                    (sign, whole, dot, fract) => double.Parse(sign + whole + dot + fract)
                );
            }

            public static IParser<char, string> _CStyleIdentifier()
            {
                var startChar = Match<char>(c => c == '_' || char.IsLetter(c));
                var bodyChar = Match<char>(c => c == '_' || char.IsLetterOrDigit(c));
                return Rule(
                    startChar,
                    bodyChar.ListCharToString(),

                    (start, rest) => start + rest
                );
            }

            public static IParser<char, double> _JavaScriptStyleNumberLiteral()
            {
                return Rule(
                    Match('-').Transform(c => "-").Optional(() => ""),
                    First(
                        Match('0').Transform(c => "0"),
                        Rule(
                            NonZeroDigit(),
                            Digit().ListCharToString(),
                            (first, rest) => first + rest
                        )
                    ),
                    First(
                        Rule(
                            Match('.').Transform(c => "."),
                            Digit().ListCharToString(true),
                            (dot, fract) => dot + fract
                        ),
                        Produce<char, string>(() => "")
                    ),
                    First(
                        Rule(
                            First(
                                Match('e').Transform(c => "e"),
                                Match('E').Transform(c => "E")
                            ),
                            First(
                                Match('+').Transform(c => "+"),
                                Match('-').Transform(c => "-"),
                                Produce<char, string>(() => "+")
                            ),
                            DigitString(),

                            (e, sign, value) => e + sign + value
                        ),
                        Produce<char, string>(() => "")
                    ),
                    (sign, whole, fract, exp) => double.Parse(sign + whole + fract + exp, NumberStyles.Float)
                );
            }

            public static IParser<char, string> _CPlusPlusStyleComment() => PrefixedLine("//");

            public static IParser<char, string> _SqlStyleComment() => PrefixedLine("--");

            public static IParser<char, string> _DoubleQuotedStringWithEscapedQuotes()
                => DelimitedStringWithEscapedDelimiters('"', '"', '\\');

            public static IParser<char, string> _SingleQuotedStringWithEscapedQuotes()
                => DelimitedStringWithEscapedDelimiters('\'', '\'', '\\');

            public static IParser<char, string> _StrippedDoubleQuotedStringWithEscapedQuotes()
                => StrippedDelimitedStringWithEscapedDelimiters('"', '"', '\\');

            public static IParser<char, string> _StrippedSingleQuotedStringWithEscapedQuotes()
                => StrippedDelimitedStringWithEscapedDelimiters('\'', '\'', '\\');
        }
    }
}