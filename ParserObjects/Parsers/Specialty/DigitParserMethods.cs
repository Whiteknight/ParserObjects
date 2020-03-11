using System;
using System.Collections.Generic;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class DigitParserMethods
    {
        /// <summary>
        /// Parses a single digit 0-9
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> Digit() => _digit.Value;
        private static readonly Lazy<IParser<char, char>> _digit = new Lazy<IParser<char, char>>(
            () => Match<char>(char.IsDigit).Named("digit")
        );

        /// <summary>
        /// Parses a single non-zero digit 1-9
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> NonZeroDigit() => _nonZeroDigit.Value;
        private static readonly Lazy<IParser<char, char>> _nonZeroDigit = new Lazy<IParser<char, char>>(
            () => Match<char>(c => c != '0' && char.IsDigit(c)).Named("nonZeroDigit")
        );

        /// <summary>
        /// Parses digits in series and returns them as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DigitString() => _digitString.Value;
        private static readonly Lazy<IParser<char, string>> _digitString = new Lazy<IParser<char, string>>(
            () => Digit().ListCharToString(true).Named("digits")
        );

        /// <summary>
        /// Returns a single hexadecimal digit: 0-9, a-f, A-F
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> HexadecimalDigit() => _hexadecimalDigit.Value;
        private static readonly HashSet<char> _hexDigits = new HashSet<char>("abcdefABCDEF0123456789");
        private static readonly Lazy<IParser<char, char>> _hexadecimalDigit = new Lazy<IParser<char, char>>(
            ()=> Match<char>(c => _hexDigits.Contains(c)).Named("hexDigit")
        );

        /// <summary>
        /// Returns a sequence of at least one hexadecimal digits and returns them as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> HexadecimalString() => _hexadecimalString.Value;
        private static readonly Lazy<IParser<char, string>> _hexadecimalString = new Lazy<IParser<char,string>>(
            () => HexadecimalDigit().ListCharToString(true).Named("hexDigits")
        );
    }
}