using static ParserObjects.Parsers<char>;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Parses a single character of whitespace (' ', '\t', '\r', '\n','\v', etc).
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> WhitespaceCharacter()
        => GetOrCreate("ws", static () => Match(char.IsWhiteSpace).Named("ws"));

    /// <summary>
    /// Parses a series of required whitespace characters and returns them as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Whitespace()
        => GetOrCreate(
            "whitespace",
            static () => WhitespaceCharacter().ListCharToString(true)
        );

    /// <summary>
    /// Parses an optional series of whitespace characters and returns them as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> OptionalWhitespace()
        => GetOrCreate(
            "whitespace?",
            static () => WhitespaceCharacter().ListCharToString(false)
        );
}
