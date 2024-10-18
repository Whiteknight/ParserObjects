using System.Collections.Generic;

namespace ParserObjects;

public static class ParserCharsExtensions
{
    /// <summary>
    /// The parser returns an array of characters. Change it to return a string instead.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<char, string> Stringify(this IParser<char, char[]> p)
        => Parsers.Stringify(p);

    /// <summary>
    /// The parser returns a read-only list of characters. Change it to return a string instead.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<char, string> Stringify(this IParser<char, IReadOnlyList<char>> p)
        => Parsers.Stringify(p);
}
