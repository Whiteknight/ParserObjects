using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class LineParserMethods
    {
        /// <summary>
        /// Parses a line of text, starting with a prefix and going until a newline. Newline not included.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IParser<char, string> PrefixedLine(string prefix)
        {
            // We should cache this, in a dictionary by prefix
            var notNewlineChar = Match<char>(c => c != '\r' && c != '\n');
            if (string.IsNullOrEmpty(prefix))
                return notNewlineChar.ListCharToString();

            var prefixParser = Match<char>(prefix).Transform(c => prefix);
            var charsParser = notNewlineChar.ListCharToString();
            return (prefixParser, charsParser)
                .Produce((p, content) => p + content)
                .Named($"linePrefixed:{prefix}");
        }
    }
}