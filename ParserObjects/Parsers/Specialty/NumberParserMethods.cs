using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers.Specialty
{
    public static class NumberParserMethods
    {
        // TODO: Parsers which are invariant should be cached for performance

        public static IParser<char, char> Digit() => Parsers.ParserMethods.Match<char>(char.IsDigit);
        public static IParser<char, string> DigitString() => Digit().List(d => new string(d.ToArray()));

        private static readonly HashSet<char> _hexDigits = new HashSet<char>("abcdefABCDEF0123456789");
        public static IParser<char, char> HexadecimalDigit() => Parsers.ParserMethods.Match<char>(c => _hexDigits.Contains(c));
        public static IParser<char, string> HexadecimalString() => HexadecimalDigit().List(x => new string(x.ToArray()));
    }
}