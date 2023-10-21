using System;
using System.Collections.Generic;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class Digits
    {
        /// <summary>
        /// Parses a single digit 0-9.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> Digit() => _digit.Value;

        private static readonly Lazy<IParser<char, char>> _digit = new Lazy<IParser<char, char>>(
            static () => Match(char.IsDigit).Named("digit")
        );

        /// <summary>
        /// Parses digits in series and returns them as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> DigitString() => _digitString.Value;

        private static readonly Lazy<IParser<char, string>> _digitString
            = new Lazy<IParser<char, string>>(
                static () => Digit().ListCharToString(true).Named("digits")
            );

        public static IParser<char, string> DigitString(int minimum, int maximum)
            => Digit().ListCharToString(minimum, maximum).Named($"digits({minimum},{maximum})");

        public static IParser<char, int> DigitsAsInteger(int minimum, int maximum)
            => DigitString(minimum, maximum).Transform(s => string.IsNullOrEmpty(s) ? 0 : int.Parse(s));

        /// <summary>
        /// Parses a single non-zero digit 1-9.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> NonZeroDigit() => _nonZeroDigit.Value;

        private static readonly Lazy<IParser<char, char>> _nonZeroDigit
            = new Lazy<IParser<char, char>>(
                static () => Match(static c => c != '0' && char.IsDigit(c)).Named("nonZeroDigit")
            );

        /// <summary>
        /// Returns a single hexadecimal digit: 0-9, a-f, A-F.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> HexadecimalDigit() => _hexadecimalDigit.Value;

        private static readonly Lazy<IParser<char, char>> _hexadecimalDigit
            = new Lazy<IParser<char, char>>(
                static () => MatchAny(new HashSet<char>("abcdefABCDEF0123456789")).Named("hexDigit")
            );

        /// <summary>
        /// Returns a sequence of at least one hexadecimal digits and returns them as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> HexadecimalString() => _hexadecimalString.Value;

        private static readonly Lazy<IParser<char, string>> _hexadecimalString
            = new Lazy<IParser<char, string>>(
                static () => HexadecimalDigit().ListCharToString(true).Named("hexDigits")
            );
    }
}
