using System.Linq;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class ParserMethods
    {
        public static IParser<char, string> PrefixedLine(string prefix)
        {
            var notNewlineChar = Match<char>(c => c != '\r' && c != '\n');
            return Rule(
                Match(prefix, c => new string(c)),
                notNewlineChar.List(l => new string(l.ToArray())),

                (p, content) => p + content
            );
        }

        public static IParser<char, char> WhitespaceCharacter() => Match<char>(char.IsWhiteSpace);

        public static IParser<char, string> Whitespace() 
            => WhitespaceCharacter().List(w => new string(w.ToArray()));

        public static IParser<char, string> RequiredWhitespace()
            => WhitespaceCharacter().List(w => new string(w.ToArray()), true);
    }
}
