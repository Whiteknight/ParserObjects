using System.Linq;

namespace ParserObjects.Parsers.Specialty
{
    public static class WhitespaceParsersMethods
    {
        /// <summary>
        /// Parses a single character of whitespace (' ', '\t', '\r', '\n','\v', etc)
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> WhitespaceCharacter()
            => ParserCache.Instance.GetParser(nameof(WhitespaceCharacter), Internal.WhitespaceCharacter);

        /// <summary>
        /// Parses a series of whitespace characters and returns them as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Whitespace()
            => ParserCache.Instance.GetParser(nameof(Whitespace), Internal.Whitespace);

        private static class Internal
        {
            public static IParser<char, char> WhitespaceCharacter() => ParserMethods.Match<char>(char.IsWhiteSpace);

            public static IParser<char, string> Whitespace()
                => WhitespaceParsersMethods.WhitespaceCharacter().List(true).Transform(w => new string(w.ToArray()));
        }
    }
}