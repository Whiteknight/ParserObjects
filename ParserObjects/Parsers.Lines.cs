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
        if (string.IsNullOrEmpty(prefix))
            return Line();

        return Capture(
                Match(prefix),
                Match(static c => c != '\n').List()
            )
            .Stringify()
            .Named($"Line Prefixed:{prefix}");
    }

    /// <summary>
    /// Parses a line of text until a newline or end of input. Newline not included.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Line() => _line.Value;

    private static readonly Lazy<IParser<char, string>> _line = new Lazy<IParser<char, string>>(
        static () =>
        {
            var notNewlineChar = Match(static c => c != '\n');
            return notNewlineChar.ListCharToString();
        }
    );
}
