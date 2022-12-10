using System.Text;
using ParserObjects.Internal.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.C;

public static class StringGrammar
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

                if (Constants.EscapableStringChars.ContainsKey(c))
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

    public static IParser<char, string> CreateCharacterParser()
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

            if (Constants.EscapableStringChars.ContainsKey(c))
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

    private static void ParseHexSequence(Sequential.State<char> s, StringBuilder sb, char typeChar)
    {
        switch (typeChar)
        {
            case 'x':
                var hex = s.Parse(Constants.HexCode);
                sb.Append('x');
                for (int i = 0; i < hex.Count; i++)
                    sb.Append(hex[i]);
                return;

            case 'u':
                var lowUCode = s.Parse(Constants.LowUnicodeCode);
                sb.Append('u');
                for (int i = 0; i < lowUCode.Count; i++)
                    sb.Append(lowUCode[i]);
                return;

            case 'U':
                var highUCode = s.Parse(Constants.HighUnicodeCode);
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
}
