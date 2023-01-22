using System;
using System.Globalization;

namespace ParserObjects;

public static partial class Parsers
{
    public static class JS
    {
        /// <summary>
        /// JavaScript-style number literal, returned as a string.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> NumberString() => _numberString.Value;

        private static readonly Lazy<IParser<char, string>> _numberString
            = new Lazy<IParser<char, string>>(Internal.Grammars.JS.NumberGrammar.CreateParser);

        /// <summary>
        /// JavaScript-style number literal returned as a parsed Double.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> Number() => _number.Value;

        private static readonly Lazy<IParser<char, double>> _number = new Lazy<IParser<char, double>>(
            () => NumberString()
                .Transform(static s => double.Parse(s, NumberStyles.Float))
                .Named("JavaScript-Style Number Literal")
        );

        /// <summary>
        /// Parse a JavaScript-style string, removing quotes and replacing escape sequences
        /// with their literal values.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> StrippedString() => _strippedString.Value;

        private static readonly Lazy<IParser<char, string>> _strippedString
            = new Lazy<IParser<char, string>>(Internal.Grammars.JS.StrippedStringGrammar.CreateParser);

        /// <summary>
        /// Parse a JavaScript-style string, returning the complete string literal with quotes
        /// and escape sequences unmodified.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> String() => _string.Value;

        private static readonly Lazy<IParser<char, string>> _string
            = new Lazy<IParser<char, string>>(Internal.Grammars.JS.StringGrammar.CreateParser);
    }
}
