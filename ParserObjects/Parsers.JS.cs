using System.Globalization;
using ParserObjects.Internal;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    public static class JS
    {
        /// <summary>
        /// JavaScript-style number literal, returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> NumberString()
            => GetOrCreate("JavaScript-Style Number String", Internal.Grammars.JS.NumberGrammar.CreateParser);

        /// <summary>
        /// JavaScript-style number literal returned as a parsed Double.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> Number()
            => GetOrCreate(
                "JavaScript-Style Number Literal",
                static () => NumberString().Transform(static s => double.Parse(s, NumberStyles.Float))
            );

        /// <summary>
        /// Parse a JavaScript-style string, removing quotes and replacing escape sequences
        /// with their literal values.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedString()
            => GetOrCreate("JavaScript-Style Stripped String", Internal.Grammars.JS.StrippedStringGrammar.CreateParser);

        /// <summary>
        /// Parse a JavaScript-style string, returning the complete string literal with quotes
        /// and escape sequences unmodified.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> String()
            => GetOrCreate("JavaScript-Style String", Internal.Grammars.JS.StringGrammar.CreateParser);
    }
}
