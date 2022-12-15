using System;
using System.Collections.Generic;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Parse a list of items.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="atLeastOne">If true, the list must have at least one element or the parse fails. If
    /// false, an empty list returns success.</param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(IParser<TInput, TOutput> p, bool atLeastOne)
        => List(p, Empty(), atLeastOne);

    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(IParser<TInput, TOutput> p, IParser<TInput> separator, bool atLeastOne)
        => List(p, separator, minimum: atLeastOne ? 1 : 0);

    /// <summary>
    /// Parse a list of items with defined minimum and maximum quantities.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(IParser<TInput, TOutput> p, int minimum, int? maximum = null)
        => new ListParser<TInput, TOutput>(p, Empty(), minimum, maximum);

    /// <summary>
    /// Parse a list of items with defined minimum and maximum quantities.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="separator"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(IParser<TInput, TOutput> p, IParser<TInput>? separator = null, int minimum = 0, int? maximum = null)
        => new ListParser<TInput, TOutput>(p, separator ?? Empty(), minimum, maximum);

    public static IParser<TInput, TOutput> NonGreedyList<TMiddle, TOutput>(IParser<TInput, TMiddle> item, IParser<TInput> separator, Func<IParser<TInput, IReadOnlyList<TMiddle>>, IParser<TInput, TOutput>> getContinuation, int minimum = 0, int? maximum = null)
        => new NonGreedyList<TInput, TMiddle, TOutput>.Parser(item, separator, getContinuation, minimum, maximum);

    public static IParser<TInput, TOutput> NonGreedyList<TMiddle, TOutput>(IParser<TInput, TMiddle> item, Func<IParser<TInput, IReadOnlyList<TMiddle>>, IParser<TInput, TOutput>> getContinuation, IParser<TInput>? separator = null, int minimum = 0, int? maximum = null)
        => new NonGreedyList<TInput, TMiddle, TOutput>.Parser(item, separator ?? Empty(), getContinuation, minimum, maximum);
}
