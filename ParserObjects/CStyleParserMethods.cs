using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ParserObjects.Internal.Parsers;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects;

public static class CStyleParserMethods
{
    private static readonly Dictionary<char, string> _escapableStringChars = new Dictionary<char, string>
    {
        { 'a', "\a" },
        { 'b', "\b" },
        { 'f', "\f" },
        { 'n', "\n" },
        { 'r', "\r" },
        { 't', "\t" },
        { 'v', "\v" },
        { '\\', "\\" },
        { '?', "?" },
        { '"', "\"" },
    };

    private static readonly IParser<char, IReadOnlyList<char>> _hexCode = HexadecimalDigit().List(2, 4);
    private static readonly IParser<char, IReadOnlyList<char>> _lowUnicodeCode = HexadecimalDigit().List(4, 4);
    private static readonly IParser<char, IReadOnlyList<char>> _highUnicodeCode = HexadecimalDigit().List(8, 8);
    private static readonly IParser<char, string> _hexCodeString = HexadecimalDigit().ListCharToString(2, 4);
    private static readonly IParser<char, string> _lowUnicodeString = HexadecimalDigit().ListCharToString(4, 4);
    private static readonly IParser<char, string> _highUnicodeString = HexadecimalDigit().ListCharToString(8, 8);

    /// <summary>
    /// C-style comment with '/*' ... '*/' delimiters.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Comment() => _comment.Value;

    private static readonly Lazy<IParser<char, string>> _comment = new Lazy<IParser<char, string>>(
        () =>
        {
            var start = Match("/*").Transform(c => "/*");
            var end = Match("*/").Transform(c => "*/");
            var standaloneAsterisk = MatchChar('*').NotFollowedBy(MatchChar('/'));
            var notAsterisk = Match(c => c != '*');

            var bodyChar = (standaloneAsterisk, notAsterisk).First();
            var bodyChars = bodyChar.ListCharToString();

            return (start, bodyChars, end)
                .Rule((s, b, e) => s + b + e)
                .Named("C-Style Comment");
        }
    );

    /// <summary>
    /// C-style hexadecimal literal returned as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> HexadecimalString() => _hexString.Value;

    private static readonly Lazy<IParser<char, string>> _hexString = new Lazy<IParser<char, string>>(
        () => (Match("0x"), HexadecimalDigit().ListCharToString(1, 8))
            .Rule((prefix, value) => prefix + value)
            .Named("C-Style Hex String")
    );

    /// <summary>
    /// C-style hexadecimal literal returned as a parsed integer.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, int> HexadecimalInteger() => _hexInteger.Value;

    private static readonly Lazy<IParser<char, int>> _hexInteger = new Lazy<IParser<char, int>>(
        () => (Match("0x"), HexadecimalDigit().ListCharToString(1, 8))
            .Rule((prefix, value) => int.Parse(value, NumberStyles.HexNumber))
            .Named("C-Style Hex Literal")
    );

    /// <summary>
    /// C-style integer literal returned as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> IntegerString() => _integerString.Value;

    private static readonly Lazy<IParser<char, string>> _integerString = new Lazy<IParser<char, string>>(
        () =>
        {
            var maybeMinus = MatchChar('-').Transform(c => "-").Optional(() => string.Empty);
            var nonZeroDigit = Match(c => char.IsDigit(c) && c != '0');
            var digits = Digit().ListCharToString();
            var zero = MatchChar('0').Transform(c => "0");
            var nonZeroNumber = (maybeMinus, nonZeroDigit, digits)
                .Rule((sign, start, body) => sign + start + body);
            return (nonZeroNumber, zero)
                .First()
                .Named("C-Style Integer String");
        }
    );

    /// <summary>
    /// C-style Integer literal returned as a parsed Int32.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, int> Integer() => _integer.Value;

    private static readonly Lazy<IParser<char, int>> _integer = new Lazy<IParser<char, int>>(
        () => IntegerString()
            .Transform(int.Parse)
            .Named("C-Style Integer Literal")
    );

    /// <summary>
    /// C-style unsigned integer literal returned as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> UnsignedIntegerString() => _unsignedIntegerString.Value;

    private static readonly Lazy<IParser<char, string>> _unsignedIntegerString = new Lazy<IParser<char, string>>(
        () =>
        {
            var nonZeroDigit = Match(c => char.IsDigit(c) && c != '0');
            var digits = Digit().ListCharToString();
            var zero = MatchChar('0').Transform(c => "0");
            var nonZeroNumber = (nonZeroDigit, digits).Rule((start, body) => start + body);
            return (nonZeroNumber, zero)
                .First()
                .Named("C-Style Unsigned Integer String");
        }
    );

    /// <summary>
    /// C-style Unsigned Integer literal returned as a parsed Int32.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, int> UnsignedInteger() => _unsignedInteger.Value;

    private static readonly Lazy<IParser<char, int>> _unsignedInteger = new Lazy<IParser<char, int>>(
        () => UnsignedIntegerString()
            .Transform(int.Parse)
            .Named("C-Style Unsigned Integer Literal")
    );

    /// <summary>
    /// C-style Double literal returned as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> DoubleString() => _doubleString.Value;

    private static readonly Lazy<IParser<char, string>> _doubleString = new Lazy<IParser<char, string>>(
        () => (IntegerString(), MatchChar('.'), DigitString())
            .Rule((whole, dot, fract) => whole + dot + fract)
            .Named("C-Style Double String")
    );

    /// <summary>
    /// C-style float/double literal returned as a parsed Double.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, double> Double() => _double.Value;

    private static readonly Lazy<IParser<char, double>> _double = new Lazy<IParser<char, double>>(
        () => DoubleString()
            .Transform(double.Parse)
            .Named("C-Style Double Literal")
    );

    /// <summary>
    /// C-style Identifier.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Identifier() => _identifier.Value;

    private static readonly Lazy<IParser<char, string>> _identifier = new Lazy<IParser<char, string>>(
        () =>
        {
            var startChar = Match(c => c == '_' || char.IsLetter(c));
            var bodyChar = Match(c => c == '_' || char.IsLetterOrDigit(c));
            var bodyChars = bodyChar.ListCharToString();
            return (startChar, bodyChars)
                .Rule((start, rest) => start + rest)
                .Named("C-Style Identifier");
        }
    );

    /// <summary>
    /// Parse a C-style string, removing quotes and replacing escape sequences with their
    /// proper values.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> StrippedString() => _strippedString.Value;

    private static readonly Lazy<IParser<char, string>> _strippedString = new Lazy<IParser<char, string>>(CreateStrippedStringParser);

    private static IParser<char, string> CreateStrippedStringParser()
    {
        var parser = Sequential(s =>
        {
            var startQuote = s.Input.Peek();
            if (startQuote != '"')
            {
                s.Fail($"Expected start quote but found '{startQuote}'");
                return "";
            }

            s.Input.GetNext();

            var sb = new StringBuilder();
            while (!s.Input.IsAtEnd)
            {
                var c = s.Input.GetNext();
                if (c == '"')
                    return sb.ToString();

                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                c = s.Input.GetNext();
                if (c >= '0' && c <= '7')
                {
                    var value = ParseStrippedOctalChar(s, c);
                    sb.Append(value);
                    continue;
                }

                if (_escapableStringChars.ContainsKey(c))
                {
                    sb.Append(_escapableStringChars[c]);
                    continue;
                }

                var hex = ParseStrippedHexChar(s, c);
                sb.Append(hex);
            }

            s.Fail("No end quote");
            return "";
        });

        return parser.Named("C-Style Stripped String");
    }

    private static char ParseStrippedHexChar(Sequential.State<char> s, char typeChar)
    {
        switch (typeChar)
        {
            case 'x':
                var hex = s.Parse(_hexCodeString);
                return (char)int.Parse(hex, NumberStyles.HexNumber);

            case 'u':
                var lowUCode = s.Parse(_lowUnicodeString);
                return char.ConvertFromUtf32(int.Parse(lowUCode, NumberStyles.HexNumber))[0];

            case 'U':
                var highUCode = s.Parse(_highUnicodeString);
                return char.ConvertFromUtf32(int.Parse(highUCode, NumberStyles.HexNumber))[0];
        }

        s.Fail($"Unknown escape sequence '{typeChar}'");
        return default;
    }

    private static char ParseStrippedOctalChar(Sequential.State<char> s, char startChar)
    {
        int value = startChar - '0';

        var c = s.Input.Peek();
        if (!(c >= '0' && c <= '7'))
            return (char)value;
        s.Input.GetNext();
        value = (value * 8) + (c - '0');

        c = s.Input.Peek();
        if (!(c >= '0' && c <= '7'))
            return (char)value;
        var biggestValue = (value * 8) + (c - '0');
        if (biggestValue >= 256)
            return (char)value;
        s.Input.GetNext();

        return (char)biggestValue;
    }

    /// <summary>
    /// Parse a C-style string, keeping quotes and escape sequences.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> String() => _string.Value;

    private static readonly Lazy<IParser<char, string>> _string = new Lazy<IParser<char, string>>(CreateStringParser);

    private static IParser<char, string> CreateStringParser()
    {
        var parser = Sequential(s =>
        {
            var startQuote = s.Input.Peek();
            if (startQuote != '"')
            {
                s.Fail($"Expected start quote but found '{startQuote}'");
                return "";
            }

            var sb = new StringBuilder();
            sb.Append(startQuote);
            s.Input.GetNext();

            while (!s.Input.IsAtEnd)
            {
                var c = s.Input.GetNext();
                if (c == '"')
                {
                    sb.Append('"');
                    return sb.ToString();
                }

                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                sb.Append('\\');
                c = s.Input.GetNext();
                if (c >= '0' && c <= '7')
                {
                    ParseOctalSequence(s, sb, c);
                    continue;
                }

                if (_escapableStringChars.ContainsKey(c))
                {
                    sb.Append(c);
                    continue;
                }

                ParseHexSequence(s, sb, c);
            }

            s.Fail("No end quote");
            return "";
        });

        return parser.Named("C-Style String");
    }

    private static void ParseHexSequence(Sequential.State<char> s, StringBuilder sb, char typeChar)
    {
        switch (typeChar)
        {
            case 'x':
                var hex = s.Parse(_hexCode);
                sb.Append('x');
                for (int i = 0; i < hex.Count; i++)
                    sb.Append(hex[i]);
                return;

            case 'u':
                var lowUCode = s.Parse(_lowUnicodeCode);
                sb.Append('u');
                for (int i = 0; i < lowUCode.Count; i++)
                    sb.Append(lowUCode[i]);
                return;

            case 'U':
                var highUCode = s.Parse(_highUnicodeCode);
                sb.Append('U');
                for (int i = 0; i < highUCode.Count; i++)
                    sb.Append(highUCode[i]);
                return;
        }

        s.Fail($"Unknown escape sequence '{typeChar}'");
    }

    private static void ParseOctalSequence(Sequential.State<char> s, StringBuilder sb, char startChar)
    {
        int value = startChar - '0';
        sb.Append(startChar);

        var c = s.Input.Peek();
        if (!(c >= '0' && c <= '7'))
            return;
        s.Input.GetNext();
        value = (value * 8) + (c - '0');
        sb.Append(c);

        c = s.Input.Peek();
        if (!(c >= '0' && c <= '7'))
            return;
        value = (value * 8) + (c - '0');
        if (value >= 256)
            return;
        s.Input.GetNext();
        sb.Append(c);
    }

    /// <summary>
    /// Parses a C-style char literal, removing quotes and resolving escape sequences.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> StrippedCharacter() => _strippedCharacter.Value;

    private static readonly Lazy<IParser<char, char>> _strippedCharacter = new Lazy<IParser<char, char>>(CreateStrippedCharacterParser);

    private static IParser<char, char> CreateStrippedCharacterParser()
    {
        var parser = Sequential(s =>
        {
            void ExpectEndQuote()
            {
                var endQuote = s.Input.GetNext();
                if (endQuote != '\'')
                    s.Fail($"Expected close quote but found '{endQuote}'");
            }

            var startQuote = s.Input.Peek();
            if (startQuote != '\'')
                s.Fail($"Expected start quote but found '{startQuote}'");

            s.Input.GetNext();
            if (s.Input.IsAtEnd)
                s.Fail("Unexpected end of input");

            var c = s.Input.GetNext();
            if (c == '\'')
                s.Fail("Unexpected close quote");

            if (c != '\\')
            {
                ExpectEndQuote();
                return c;
            }

            c = s.Input.GetNext();
            if (c >= '0' && c <= '7')
            {
                var value = ParseStrippedOctalChar(s, c);
                ExpectEndQuote();
                return value;
            }

            if (c == '\'')
            {
                ExpectEndQuote();
                return c;
            }

            if (_escapableStringChars.ContainsKey(c))
            {
                ExpectEndQuote();
                return _escapableStringChars[c][0];
            }

            var hex = ParseStrippedHexChar(s, c);
            ExpectEndQuote();
            return hex;
        });

        return parser.Named("C-Style Stripped Character");
    }

    /// <summary>
    /// Parse a C-style char literal, keeping the quotes and escape sequences.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Character() => _character.Value;

    private static readonly Lazy<IParser<char, string>> _character = new Lazy<IParser<char, string>>(CreateCharacterParser);

    private static IParser<char, string> CreateCharacterParser()
    {
        var parser = Sequential(s =>
        {
            var startQuote = s.Input.Peek();
            if (startQuote != '\'')
                s.Fail($"Expected start quote but found '{startQuote}'");
            s.Input.GetNext();

            var sb = new StringBuilder();
            sb.Append(startQuote);
            void ExpectEndQuote()
            {
                var endQuote = s.Input.GetNext();
                if (endQuote != '\'')
                    s.Fail($"Expected close quote but found '{endQuote}'");
                sb.Append(endQuote);
            }

            if (s.Input.IsAtEnd)
                s.Fail("Unexpected end of input");

            var c = s.Input.GetNext();
            if (c == '\'')
                s.Fail("Unexpected close quote");

            if (c != '\\')
            {
                sb.Append(c);
                ExpectEndQuote();
                return sb.ToString();
            }

            sb.Append('\\');
            c = s.Input.GetNext();
            if (c >= '0' && c <= '3')
            {
                ParseOctalSequence(s, sb, c);
                ExpectEndQuote();
                return sb.ToString();
            }

            if (c == '\'')
            {
                sb.Append(c);
                ExpectEndQuote();
                return sb.ToString();
            }

            if (_escapableStringChars.ContainsKey(c))
            {
                sb.Append(c);
                ExpectEndQuote();
                return sb.ToString();
            }

            ParseHexSequence(s, sb, c);
            ExpectEndQuote();
            return sb.ToString();
        });

        return parser.Named("C-Style Character");
    }
}
