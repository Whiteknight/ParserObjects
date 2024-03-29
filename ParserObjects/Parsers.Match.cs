﻿using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal;
using ParserObjects.Internal.Parsers;

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
    /// Returns a success result at end of input, a failure result at any other location. Returns
    /// no value in either case.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput> End() => _end;

    /// <summary>
    /// Always returns success, with a boolean value indicating whether the input sequence is at
    /// end of input or not.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, bool> IsEnd() => _isEnd;

    /// <summary>
    /// Test the next input value and return it, if it matches the predicate. Notice that this
    /// parser can match the end sentinel, if the end sentinel satisfies the given predicate.
    /// If you do not want to match the end sentinel, update your predicate to exclude it or use
    /// MatchItem(predicate) instead.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<TInput, TInput> Match(Func<TInput, bool> predicate)
        => new MatchPredicateParser<TInput, Func<TInput, bool>>(predicate, static (i, p) => p(i));

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
        if (pattern == null)
            return Produce(static () => Array.Empty<TInput>());
        var asList = pattern as IReadOnlyList<TInput> ?? pattern.ToList();
        if (asList.Count == 0)
            return Produce(static () => Array.Empty<TInput>());
        return new MatchPatternParser<TInput>(asList);
    }

    /// <summary>
    /// Test the next input value and return it, if it matches the predicate. Notice that this
    /// parser cannot match the end sentinel, even if the given predicate would allow it. If you
    /// do want to match the end sentinel, use Match(predicate) instead.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<TInput, TInput> MatchItem(Func<TInput, bool> predicate)
        => new MatchPredicateParser<TInput, Func<TInput, bool>>(predicate, static (i, p) => p(i), readAtEnd: false);

    /// <summary>
    /// Return the next item of input without consuming any input. Returns failure at end of
    /// input, success otherwise.
    /// </summary>
    /// <returns></returns>
    public static IParser<TInput, TInput> Peek() => _peek;
}
