using System.Globalization;
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
                DigitString(),
                (sign, i) => int.Parse(sign + i)
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

        // TODO: C-style string literals with escapes
        // TODO: C-style char literals with escapes
        // TODO: JS-style string literals with escapes (https://mathiasbynens.be/notes/javascript-escapes)

        public static IParser<char, string> CPlusPlussStyleComment() => PrefixedLine("//");

        public static IParser<char, string> SqlStyleComment() => PrefixedLine("--");
    }
}