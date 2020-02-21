using System.Linq;
using ParserObjects.Utility;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class WhitespaceParserMethods
    {
        /// <summary>
        /// Parses a single character of whitespace (' ', '\t', '\r', '\n','\v', etc)
        /// </summary>
        /// <returns></returns>
        public static IParser<char, char> WhitespaceCharacter()
            => ParserCache.Instance.GetParser(nameof(WhitespaceCharacter), Internal._WhitespaceCharacter);

        /// <summary>
        /// Parses a series of whitespace characters and returns them as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> Whitespace()
            => ParserCache.Instance.GetParser(nameof(Whitespace), Internal._Whitespace);

        private static class Internal
        {
            public static IParser<char, char> _WhitespaceCharacter() => Match<char>(char.IsWhiteSpace);

            public static IParser<char, string> _Whitespace()
                => WhitespaceCharacter().List(true).Transform(w => new string(w.ToArray()));
        }
    }
}