using System.Collections.Generic;
using static ParserObjects.Internal.ParserCache;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Parses a single digit 0-9.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> Digit()
        => GetOrCreate("digit", static () => Match(char.IsDigit));

    /// <summary>
    /// Parses digits in series and returns them as a string with given minimum and maximum
    /// sizes.
    /// </summary>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<char, string> DigitString(int minimum, int maximum)
        => Digit().ListCharToString(minimum, maximum).Named($"digits({minimum},{maximum})");

    /// <summary>
    /// Parses digits in series and returns them as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> DigitString()
        => GetOrCreate("digits", static () => Digit().ListCharToString(true));

    /// <summary>
    /// Parses digits in series, from 0-999,999,999 inclusive, and returns them as an integer.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, int> DigitsAsInteger() => DigitsAsInteger(1, 9);

    /// <summary>
    /// Parses digits in series and returns them as an integer with a given minimum and maximum
    /// number of digits.
    /// </summary>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<char, int> DigitsAsInteger(int minimum, int maximum)
        => DigitString(minimum, maximum).Transform(s => string.IsNullOrEmpty(s) ? 0 : int.Parse(s));

    /// <summary>
    /// Parses a single non-zero digit 1-9.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> NonZeroDigit()
        => GetOrCreate("nonZeroDigit", static () => Match(static c => c != '0' && char.IsDigit(c)));

    /// <summary>
    /// Returns a single hexadecimal digit: 0-9, a-f, A-F.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> HexadecimalDigit()
        => GetOrCreate("hexDigit", static () => MatchAny(new HashSet<char>("abcdefABCDEF0123456789")));

    /// <summary>
    /// Returns a sequence of at least one hexadecimal digits and returns them as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> HexadecimalString()
        => GetOrCreate("hexDigits", static () => HexadecimalDigit().ListCharToString(true));

    public static IParser<char, int> HexadecimalDigitsAsInteger(int minimum, int maximum)
        => HexadecimalDigit()
            .ListCharToString(minimum, maximum)
            .Transform(s => string.IsNullOrEmpty(s) ? 0 : int.Parse(s, System.Globalization.NumberStyles.HexNumber));
}
