using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Regexes;
using ParserObjects.Internal.Regexes.Patterns;
using ParserObjects.Regexes;
using static ParserObjects.Internal.ParserCache;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Creates a parser which attempts to match the given regular expression from the current
    /// position of the input stream.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    /// <exception cref="RegexException">Thrown if the pattern is invalid.</exception>
    public static IParser<char, string> Regex(string pattern)
        => string.IsNullOrEmpty(pattern)
        ? Parsers<char>.Produce(static () => string.Empty)
        : ParseRegexPattern(pattern);

    /// <summary>
    /// Creates a parser which attempts to match the given regular expression from the current
    /// position of the input stream.
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="parsers"></param>
    /// <returns></returns>
    /// <exception cref="RegexException">Thrown if the pattern is invalid.</exception>
    public static IParser<char, string> Regex(string pattern, params IParser<char>[] parsers)
        => string.IsNullOrEmpty(pattern)
        ? Parsers<char>.Produce(static () => string.Empty)
        : ParseRegexPattern(pattern, parsers);

    /// <summary>
    /// Creates a parser which attempts to match the given regular expression from the current
    /// psition of the input stream and returns the RegexMatch object.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    /// <exception cref="RegexException">Thrown if the pattern is invalid.</exception>
    public static IParser<char, RegexMatch> RegexMatch(string pattern)
        => string.IsNullOrEmpty(pattern)
        ? Parsers<char>.Produce(static () => Regexes.RegexMatch.Empty)
        : ParseRegexPattern(pattern);

    /// <summary>
    /// Creates a parser which attempts to match the given regular expression from the current
    /// psition of the input stream and returns the RegexMatch object.
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="parsers"></param>
    /// <returns></returns>
    /// <exception cref="RegexException">Thrown if the pattern is invalid.</exception>
    public static IParser<char, RegexMatch> RegexMatch(string pattern, params IParser<char>[] parsers)
        => string.IsNullOrEmpty(pattern)
        ? Parsers<char>.Produce(static () => Regexes.RegexMatch.Empty)
        : ParseRegexPattern(pattern, parsers);

    /// <summary>
    /// Creates a parser which parses a regex pattern string into a Regex object for work with
    /// the RegexParser and RegexEngine.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, Regex> RegexPattern()
        => GetOrCreate("Regex Pattern", RegexPatternGrammar.CreateParser);

    private static RegexParser ParseRegexPattern(string pattern)
        => RegexPattern().Parse(pattern, SequenceOptions.ForRegex(pattern)) switch
        {
            (true, var value, _) => new RegexParser(value, pattern),
            (false, _, var error) => throw new RegexException($"Could not parse pattern {pattern} error: {error}")
        };

    private static RegexParser ParseRegexPattern(string pattern, IParser<char>[] parsers)
        => RegexPattern()
            .WithDataContext(parsers, static (data, ps) =>
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    if (!string.IsNullOrEmpty(ps[i].Name))
                        data.Set(ps[i].Name, ps[i]);
                }
            })
            .Parse(pattern, SequenceOptions.ForRegex(pattern))
        switch
        {
            (true, var value, _) => new RegexParser(value, pattern),
            (false, _, var error) => throw new RegexException($"Could not parse pattern {pattern} error: {error}")
        };
}
