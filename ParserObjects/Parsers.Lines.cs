using System;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Parses a line of text, starting with a prefix and going until a newline or end
    /// of input. Newline not included.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static IParser<char, string> PrefixedLine(string prefix)
    {
        // We should cache this, in a dictionary by prefix
        var notNewlineChar = Match(c => c != '\n');
        if (string.IsNullOrEmpty(prefix))
            return Line();

        var prefixParser = Match(prefix).Transform(c => prefix);
        var charsParser = notNewlineChar.ListCharToString();
        return (prefixParser, charsParser)
            .Rule((p, content) => p + content)
            .Named($"linePrefixed:{prefix}");
    }

    /// <summary>
    /// Parses a line of text until a newline or end of input. Newline not included.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Line() => _line.Value;

    private static readonly Lazy<IParser<char, string>> _line = new Lazy<IParser<char, string>>(
        static () =>
        {
            var notNewlineChar = Match(c => c != '\n');
            return notNewlineChar.ListCharToString();
        }
    );
}
