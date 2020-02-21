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
            var notNewlineChar = Match<char>(c => c != '\r' && c != '\n');
            if (string.IsNullOrEmpty(prefix))
                return notNewlineChar.ListCharToString();

            return Rule(
                Match<char>(prefix).Transform(c => prefix),
                notNewlineChar.ListCharToString(),

                (p, content) => p + content
            );
        }
    }
}