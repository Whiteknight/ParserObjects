using System.Globalization;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.JS;

public static class StrippedStringGrammar
{
    public static IParser<char, string> CreateParser()
    {
        var escapeCharacter = Match(c => Constants.EscapableStringCharacters.ContainsKey(c))
            .Transform(c => Constants.EscapableStringCharacters[c]);

        var hexSequence = Rule(
            MatchChar('x'),
            HexadecimalDigit().ListCharToString(2, 2),
            static (_, hex) => ((char)int.Parse(hex, NumberStyles.HexNumber)).ToString()
        );

        var unicodeEscapeSequence = Rule(
            MatchChar('u'),
            HexadecimalDigit().ListCharToString(4, 4),
            static (_, hex) => char.ConvertFromUtf32(int.Parse(hex, NumberStyles.HexNumber))
        );

        var unicodeCodePointEscapeSequence = Rule(
            MatchChars("u{"),
            HexadecimalDigit().ListCharToString(1, 8),
            MatchChar('}'),
            static (_, hex, _) => char.ConvertFromUtf32(int.Parse(hex, NumberStyles.HexNumber))
        );

        var escapeSequenceForSingleQuotedString = Rule(
            MatchChar('\\'),
            First(
                MatchChar('\n').Transform(_ => ""),
                MatchChar('\'').Transform(_ => "'"),
                escapeCharacter,
                hexSequence,
                unicodeEscapeSequence,
                unicodeCodePointEscapeSequence
            ),
            static (_, escape) => escape
        );

        var bodyCharForSingleQuotedString = First(
            escapeSequenceForSingleQuotedString,
            Match(static c => c != '\\' && c != '\'').Transform(c => c.ToString())
        );

        var singleQuotedString = Rule(
            MatchChar('\''),
            bodyCharForSingleQuotedString.ListStringsToString(),
            MatchChar('\''),
            static (_, body, _) => body
        ).Named("JavaScript-Style Single-Quoted Stripped String");

        var escapeSequenceForDoubleQuotedString = Rule(
            MatchChar('\\'),
            First(
                MatchChar('\n').Transform(_ => ""),
                MatchChar('"').Transform(_ => "\""),
                escapeCharacter,
                hexSequence,
                unicodeEscapeSequence,
                unicodeCodePointEscapeSequence
            ),
            static (_, escape) => escape
        );

        var bodyCharForDoubleQuotedString = First(
            escapeSequenceForDoubleQuotedString,
            Match(c => c != '\\' && c != '"').Transform(c => c.ToString())
        );

        var doubleQuotedString = Rule(
            MatchChar('"'),
            bodyCharForDoubleQuotedString.ListStringsToString(),
            MatchChar('"'),
            static (_, body, _) => body
        ).Named("JavaScript-Style Double-Quoted Stripped String");

        return First(
            doubleQuotedString,
            singleQuotedString
        ).Named("JavaScript-Style Stripped String");
    }
}
