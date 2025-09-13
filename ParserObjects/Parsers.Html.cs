using static ParserObjects.Internal.ParserCache;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class Html
    {
        public static IParser<char, string> ColorCode()
            => GetOrCreate("HTML Color Code", static () => CaptureString(
                MatchChar('#'),
                HexadecimalDigit().List(6)));

        public static IParser<char, System.Drawing.Color> Color()
            => GetOrCreate("HTML Color", static () => Rule(
                MatchChar('#'),
                HexadecimalDigitsAsInteger(2, 2),
                HexadecimalDigitsAsInteger(2, 2),
                HexadecimalDigitsAsInteger(2, 2),
                (_, r, g, b) => System.Drawing.Color.FromArgb(r, g, b)));
    }
}
