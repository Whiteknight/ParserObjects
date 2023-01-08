using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Test the next input value and return it, if it matches the predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<TInput, TInput> Match(Func<TInput, bool> predicate)
        => new MatchPredicateParser<TInput>(predicate);

    /// <summary>
    /// Get the next input value and return it if it .Equals() to the given value.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static IParser<TInput, TInput> Match(TInput pattern)
        => new MatchItemParser<TInput>(pattern);

    /// <summary>
    /// Get the next few input values and compare them one-by-one against an ordered sequence of test
    /// values. If every value in the sequence matches, return the sequence as a list.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TInput>> Match(IEnumerable<TInput> pattern)
    {
        var asList = pattern.ToList();
        if (asList.Count == 0)
            return Produce(() => Array.Empty<TInput>());
        return new MatchPatternParser<TInput>(asList);
    }
}
