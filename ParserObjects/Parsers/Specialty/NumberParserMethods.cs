using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class NumberParserMethods
    {
        // TODO: Parsers which are invariant should be cached for performance

        public static IParser<char, char> Digit() => Match<char>(char.IsDigit);
        public static IParser<char, char> NonZeroDigit() => Match<char>(c =>  c != '0' && char.IsDigit(c));

        public static IParser<char, string> DigitString() => Digit().List(d => new string(d.ToArray()), true);

        private static readonly HashSet<char> _hexDigits = new HashSet<char>("abcdefABCDEF0123456789");
        public static IParser<char, char> HexadecimalDigit() => Match<char>(c => _hexDigits.Contains(c));
        public static IParser<char, string> HexadecimalString() => HexadecimalDigit().List(x => new string(x.ToArray()));
    }
}