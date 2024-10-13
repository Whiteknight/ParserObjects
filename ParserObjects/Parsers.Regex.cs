using System;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Regexes;
using ParserObjects.Regexes;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Creates a parser which attempts to match the given regular expression from the current
    /// position of the input stream.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static IParser<char, string> Regex(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return Parsers<char>.Produce(static () => string.Empty);

        var result = RegexPattern().Parse(pattern, new SequenceOptions<char>
        {
            FileName = pattern,
            MaintainLineEndings = true
        });
        return result.Success
            ? (IParser<char, string>)new RegexParser(result.Value, pattern)
            : throw new RegexException("Could not parse pattern " + pattern);
    }

    /// <summary>
    /// Creates a parser which parses a regex pattern string into a Regex object for work with
    /// the RegexParser and RegexEngine.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Regex> RegexPattern() => _regexPattern.Value;

    private static readonly Lazy<IParser<char, Regex>> _regexPattern
        = new Lazy<IParser<char, Regex>>(RegexPatternGrammar.CreateParser);
}
