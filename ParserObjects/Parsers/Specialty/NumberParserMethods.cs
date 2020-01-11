using System;
using System.Collections.Generic;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class NumberParserMethods
    {
        // TODO: Parsers which are invariant should be cached for performance

        public static IParser<char, char> Digit() 
            => ParserCache.Instance.GetParser(nameof(Digit), () => Match<char>(char.IsDigit));

        private static IParser<char, char> NonZeroDigitInternal()
            => Match<char>(c => c != '0' && char.IsDigit(c));
        public static IParser<char, char> NonZeroDigit() 
            => ParserCache.Instance.GetParser(nameof(NonZeroDigit), NonZeroDigitInternal);

        public static IParser<char, string> DigitString() 
            => ParserCache.Instance.GetParser(nameof(DigitString), () => Digit().ListCharToString(true));

        private static readonly HashSet<char> _hexDigits = new HashSet<char>("abcdefABCDEF0123456789");
        private static IParser<char, char> HexadecimalDigitInternal() 
            => Match<char>(c => _hexDigits.Contains(c));
        public static IParser<char, char> HexadecimalDigit() 
            => ParserCache.Instance.GetParser(nameof(HexadecimalDigit), HexadecimalDigitInternal);

        private static IParser<char, string> HexadecimalStringInternal() 
            => HexadecimalDigit().ListCharToString();
        public static IParser<char, string> HexadecimalString() 
            => ParserCache.Instance.GetParser(nameof(HexadecimalString), HexadecimalStringInternal);
    }
}