using System;
using System.Collections.Generic;
using ParserObjects.Utility;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class NumberParserMethods
    {
        /// <summary>
        /// Parses a single digit 0-9
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> Digit() 
            => ParserCache.Instance.GetParser(nameof(Digit), () => Match<char>(char.IsDigit));

        /// <summary>
        /// Parses a single non-zero digit 1-9
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> NonZeroDigit() 
            => ParserCache.Instance.GetParser(nameof(NonZeroDigit), Internal.NonZeroDigit);

        /// <summary>
        /// Parses digits in series and returns them as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DigitString() 
            => ParserCache.Instance.GetParser(nameof(DigitString), () => Digit().ListCharToString(true));

        /// <summary>
        /// Returns a single hexadecimal digit: 0-9, a-f, A-F
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> HexadecimalDigit() 
            => ParserCache.Instance.GetParser(nameof(HexadecimalDigit), Internal.HexadecimalDigit);

        /// <summary>
        /// Returns a sequence of hexadecimal digits and returns them as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> HexadecimalString() 
            => ParserCache.Instance.GetParser(nameof(HexadecimalString), Internal.HexadecimalString);

        private static class Internal
        {
            public static IParser<char, char> NonZeroDigit() 
                => Match<char>(c => c != '0' && char.IsDigit(c));

            private static readonly HashSet<char> _hexDigits = new HashSet<char>("abcdefABCDEF0123456789");
            public static IParser<char, char> HexadecimalDigit()
                => Match<char>(c => _hexDigits.Contains(c));

            public static IParser<char, string> HexadecimalString()
                => HexadecimalDigit().ListCharToString();
        }
    }
}