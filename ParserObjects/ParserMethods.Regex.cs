using System;
using ParserObjects.Parsers;
using ParserObjects.Regexes;

namespace ParserObjects;

public static partial class ParserMethods
{
    /// <summary>
    /// Creates a parser which attempts to match the given regular expression from the current
    /// position of the input stream.
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="maxItems">Maximum number of items to keep in the buffer</param>
    /// <returns></returns>
    public static IParser<char, string> Regex(string pattern, int maxItems = 0)
    {
        var regexParser = RegexPattern();
        var result = regexParser.Parse(pattern);
        if (!result.Success)
            throw new RegexException("Could not parse pattern " + pattern);

        return new RegexParser(result.Value, pattern, maxItems);
    }

    /// <summary>
    /// Creates a parser which parses a regex pattern string into a Regex object for work with
    /// the RegexParser and RegexEngine.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Regex> RegexPattern() => _regexPattern.Value;

    private static readonly Lazy<IParser<char, Regex>> _regexPattern = new Lazy<IParser<char, Regex>>(RegexPatternGrammar.CreateParser);
}
