using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;

namespace ParserObjects;

// Parsers which match input values.
public static partial class Parsers<TInput>
{
    private static readonly IParser<TInput, TInput> _any = new AnyParser<TInput>();
    private static readonly IParser<TInput> _empty = new EmptyParser<TInput>();
    private static readonly IParser<TInput> _end = new EndParser<TInput>();
    private static readonly IParser<TInput, TInput> _peek = new PeekParser<TInput>();

    private static readonly IParser<TInput, bool> _isEnd = new Function<TInput, bool>.Parser<object>(
        Defaults.ObjectInstance,
        static (state, _, args) => state.Input.IsAtEnd ? args.Success(true) : args.Failure(""),
        static (state, _) => state.Input.IsAtEnd,
        "IF END THEN PRODUCE",
        Array.Empty<IParser>()
    );

    /// <summary>
    /// Matches anywhere in the sequence except at the end, and consumes 1 token of input.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, TInput> Any() => _any;

    /// <summary>
    /// The empty parser, consumers no input and always returns success at any point.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput> Empty() => _empty;

    /// <summary>
    /// Matches affirmatively at the end of the input, fails everywhere else. Returns no value.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput> End() => _end;

    /// <summary>
    /// Matches affirmatively at the end of the input. Fails everywhere else. Returns a boolean value.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, bool> IsEnd() => _isEnd;

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

    /// <summary>
    /// Return the next item of input without consuming any input. Returns failure at end of
    /// input, success otherwise.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, TInput> Peek() => _peek;
}
