using System.Linq;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class ParserMethods
    {
        /// <summary>
        /// Parses a line of text, starting with a prefix and going until a newline
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IParser<char, string> PrefixedLine(string prefix)
        {
            var notNewlineChar = Match<char>(c => c != '\r' && c != '\n');
            return Rule(
                Match<char>(prefix).Transform(c => prefix),
                notNewlineChar.List().Transform(l => new string(l.ToArray())),

                (p, content) => p + content
            );
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> RequiredWhitespace()
            => ParserCache.Instance.GetParser(nameof(RequiredWhitespace), Internal.RequiredWhitespace);

        private static class Internal
        {
            public static IParser<char, char> WhitespaceCharacter() => Match<char>(char.IsWhiteSpace);

            public static IParser<char, string> Whitespace()
                => ParserMethods.WhitespaceCharacter().List().Transform(w => new string(w.ToArray()));

            public static IParser<char, string> RequiredWhitespace()
                => ParserMethods.WhitespaceCharacter().List(true).Transform(w => new string(w.ToArray()));
        }
    }
}
