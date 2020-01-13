using System.Linq;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{

    public static class LineParserMethods
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
    }
}