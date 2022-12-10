using System.Globalization;
using System.Text;
using ParserObjects.Internal.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.C;

public static class StrippedStringGrammar
{
    public static IParser<char, string> CreateStringParser()
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

                ParseStrippedStringEscapeSequence(s, sb, c);
            }

            s.Fail("No end quote");
            return "";
        });
        return parser.Named("C-Style Stripped String");
    }

    public static IParser<char, char> CreateCharacterParser()
    {
        var parser = Sequential(s =>
        {
            CheckForCharPreamble(s);

            var c = s.Input.GetNext();
            if (c == '\'')
                s.Fail("Unexpected close quote");

            if (c != '\\')
            {
                ExpectEndQuote(s);
                return c;
            }

            var value = ParseStrippedCharacterEscapeSequence(s);
            ExpectEndQuote(s);
            return value;
        });

        return parser.Named("C-Style Stripped Character");
    }

    private static void CheckForCharPreamble(Sequential.State<char> s)
    {
        var startQuote = s.Input.Peek();
        if (startQuote != '\'')
            s.Fail($"Expected start quote but found '{startQuote}'");

        s.Input.GetNext();
        if (s.Input.IsAtEnd)
            s.Fail("Unexpected end of input");
    }

    private static void ExpectEndQuote(Sequential.State<char> s)
    {
        var endQuote = s.Input.GetNext();
        if (endQuote != '\'')
            s.Fail($"Expected close quote but found '{endQuote}'");
    }

    private static char ParseStrippedCharacterEscapeSequence(Sequential.State<char> s)
    {
        var c = s.Input.GetNext();
        if (c >= '0' && c <= '7')
            return ParseStrippedOctalChar(s, c);

        if (c == '\'')
            return c;

        if (Constants.EscapableStringChars.ContainsKey(c))
            return Constants.EscapableStringChars[c][0];

        return ParseStrippedHexChar(s, c);
    }

    private static void ParseStrippedStringEscapeSequence(Sequential.State<char> s, StringBuilder sb, char c)
    {
        c = s.Input.GetNext();
        if (c >= '0' && c <= '7')
        {
            var value = ParseStrippedOctalChar(s, c);
            sb.Append(value);
            return;
        }

        if (Constants.EscapableStringChars.ContainsKey(c))
        {
            sb.Append(Constants.EscapableStringChars[c]);
            return;
        }

        var hex = ParseStrippedHexChar(s, c);
        sb.Append(hex);
    }

    private static char ParseStrippedHexChar(Sequential.State<char> s, char typeChar)
    {
        switch (typeChar)
        {
            case 'x':
                var hex = s.Parse(Constants.HexCodeString);
                return (char)int.Parse(hex, NumberStyles.HexNumber);

            case 'u':
                var lowUCode = s.Parse(Constants.LowUnicodeString);
                return char.ConvertFromUtf32(int.Parse(lowUCode, NumberStyles.HexNumber))[0];

            case 'U':
                var highUCode = s.Parse(Constants.HighUnicodeString);
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
}
