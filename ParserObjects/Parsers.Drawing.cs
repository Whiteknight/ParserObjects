using System;
using System.Drawing;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    public static class Drawing
    {
        public static IParser<char, ConsoleColor> ConsoleColor()
            => CSharp.Enum<ConsoleColor>();

        public static IParser<char, Color> Color()
            => First(
                CSharp.Enum<KnownColor>().Transform(System.Drawing.Color.FromKnownColor),
                Html.Color());
    }
}
