using static ParserObjects.Parsers<char>;
using static ParserObjects.Parsers;

namespace ParserObjects.Internal.Grammars.Sql;

public static class IdentifierGrammar
{
    public static IParser<char, string> CreateParser()
    {
        return First(
            CaptureString(
                Match(static c => char.IsLetter(c) || c == '_' || c == '@' || c == '#'),
                Match(static c => char.IsLetterOrDigit(c) || c == '_' || c == '@' || c == '#' || c == '$').List()
            ),
            // T-SQL style [] delimited identifiers
            Rule(
                MatchChar('['),
                CaptureString(
                    Match(static c => char.IsLetterOrDigit(c) || c == ' ' || (char.IsSymbol(c) && c != ']' && c != '[')).List()
                ),
                MatchChar(']'),
                static (_, id, _) => id
            ),
            // Oracle-style single-quoted identifer
            Rule(
                MatchChar('\''),
                CaptureString(
                    Match(static c => char.IsLetterOrDigit(c) || c == ' ' || (char.IsSymbol(c) && c != '\'')).List()
                ),
                MatchChar('\''),
                static (_, id, _) => id
            ),
            // Oracle-style double-quoted identifer
            Rule(
                MatchChar('"'),
                First(
                    MatchChars("\"\"").Transform(static _ => '"'),
                    Match(static c => char.IsLetterOrDigit(c) || c == ' ' || (char.IsSymbol(c) && c != '"'))
                ).ListCharToString(),
                MatchChar('"'),
                static (_, id, _) => id
            )
        );
    }
}
