using System.Globalization;
using System.Linq;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Specialty.NumberParserMethods;
using static ParserObjects.Parsers.Specialty.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class ProgrammingParserMethods
    {
        public static IParser<char, string> CStyleComment() => new CStyleCommentParser();

        public static IParser<char, int> CStyleHexadecimalLiteral()
        {
            return Rule(
                Match("0x", c => c),
                HexadecimalString(),

                (prefix, value) => int.Parse(value, NumberStyles.HexNumber));
        }

        public static IParser<char, int> CStyleIntegerLiteral()
        {
            return Rule(
                Optional(Match("-", c => "-"), () => ""),
                Digit().List(d => new string(d.ToArray())),

                (sign, value) => int.Parse(sign + value)
            );
        }

        public static IParser<char, double> CStyleDoubleLiteral()
        {
            return Rule(
                Optional(Match("-", c => "-"), () => ""),
                DigitString(),
                Match(".", c => "."),
                DigitString(),

                (sign, whole, dot, fract) => double.Parse(sign + whole + dot + fract)
            );
        }

        public static IParser<char, string> CStyleIdentifier()
        {
            var startChar = Match<char>(c => c == '_' || char.IsLetter(c));
            var bodyChar = Match<char>(c => c == '_' || char.IsLetterOrDigit(c));
            return Rule(
                startChar,
                bodyChar.List(c => new string(c.ToArray())),

                (start, rest) => start + rest
            );
        }

        public static IParser<char, double> JavaScriptStyleNumberLiteral()
        {
            return Rule(
                Optional(Match("-", c => "-"), () => ""),
                First(
                    Match("0", c => "0"),
                    Rule(
                        NonZeroDigit(),
                        Digit().List(d => new string(d.ToArray())),
                        (first, rest) => first + rest
                    )
                ),
                First(
                    Rule(
                        Match(".", c => "."),
                        Digit().List(d => new string(d.ToArray()), true),
                        (dot, fract) => dot + fract
                    ),
                    Produce<char, string>(() => "")
                ),
                First(
                    Rule(
                        First(
                            Match("e", c => "e"),
                            Match("E", c => "E")
                        ),
                        First(
                            Match("+", c => "+"),
                            Match("-", c => "-"),
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

        // TODO: C-style string literals with escapes
        // TODO: C-style char literals with escapes
        // TODO: JS-style string literals with escapes (https://mathiasbynens.be/notes/javascript-escapes)

        public static IParser<char, string> CPlusPlusStyleComment() => PrefixedLine("//");

        public static IParser<char, string> SqlStyleComment() => PrefixedLine("--");

        public static IParser<char, string> DoubleQuotedStringWithEscapedQuotes()
            => DelimitedStringWithEscapedDelimiters('"', '"', '\\');

        public static IParser<char, string> SingleQuotedStringWithEscapedQuotes()
            => DelimitedStringWithEscapedDelimiters('\'', '\'', '\\');

        public static IParser<char, string> DelimitedStringWithEscapedDelimiters(char openStr, char closeStr, char escapeStr)
        {
            // TODO: Once we enter into a string and pass the opening char, we can't backtrack out of it
            var bodyChar = First(
                Match(escapeStr.ToString() + closeStr , c => escapeStr.ToString() + closeStr),
                Match<char>(c => c != closeStr).Transform(c => c.ToString())
            );
            return Rule(
                Match(openStr.ToString(), c => openStr.ToString()),
                bodyChar.List(l => string.Join("", l)),
                Match(closeStr.ToString(), c => closeStr.ToString()),

                (open, body, close) => open + body + close
            );
        }

        public static IParser<char, string> StrippedDoubleQuotedStringWithEscapedQuotes()
            => StrippedDelimitedStringWithEscapedDelimiters('"', '"', '\\');

        public static IParser<char, string> StrippedSingleQuotedStringWithEscapedQuotes()
            => StrippedDelimitedStringWithEscapedDelimiters('\'', '\'', '\\');

        public static IParser<char, string> StrippedDelimitedStringWithEscapedDelimiters(char openStr, char closeStr, char escapeStr)
        {
            // TODO: Once we enter into a string and pass the opening char, we can't backtrack out of it
            var bodyChar = First(
                Match(escapeStr.ToString() + closeStr, c => closeStr),
                Match<char>(c => c != closeStr)
            );
            return Rule(
                Match(openStr.ToString(), c => openStr),
                bodyChar.List(l => new string(l.ToArray())),
                Match(closeStr.ToString(), c => closeStr),

                (open, body, close) => body
            );
        }
    }
}