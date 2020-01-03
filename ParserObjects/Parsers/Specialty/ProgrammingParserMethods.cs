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
    }
}