using System.Collections.Generic;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Parse a list of items. Specify whether at least one item must match, or if the list may be
    /// empty.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="atLeastOne">If true, the list must have at least one element or the parse fails. If
    /// false, an empty list returns success.</param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(IParser<TInput, TOutput> p, bool atLeastOne)
        => List(p, Empty(), atLeastOne);

    /// <summary>
    /// Parse a list of items. Specify whether at least one item must match, or if the list may be
    /// empty.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="atLeastOne"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> List(IParser<TInput> p, bool atLeastOne)
        => List(p, Empty(), atLeastOne);

    /// <summary>
    /// Parse a list of items with a separator between them. Specify whether at least one item must
    /// match, or if the list may be empty.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="separator"></param>
    /// <param name="atLeastOne"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(
        IParser<TInput, TOutput> p,
        IParser<TInput> separator,
        bool atLeastOne
    ) => List(p, separator, minimum: atLeastOne ? 1 : 0);

    /// <summary>
    /// Parse a list of items with a separator in between. Specify whether at least one item must
    /// match, or if the list may be empty.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="separator"></param>
    /// <param name="atLeastOne"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> List(IParser<TInput> p, IParser<TInput> separator, bool atLeastOne)
        => List(p, separator, minimum: atLeastOne ? 1 : 0);

    /// <summary>
    /// Parse a list of items with defined minimum and maximum quantities.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(
        IParser<TInput, TOutput> p,
        int minimum,
        int? maximum = null
    ) => List(p, Empty(), minimum, maximum);

    /// <summary>
    /// Parse a list of items with defined minimum and maximum quantities.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> List(IParser<TInput> p, int minimum, int? maximum = null)
        => List(p, Empty(), minimum, maximum);

    /// <summary>
    /// Parse a list of items with separator in between and defined minimum and maximum quantities.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="separator"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<TOutput>> List<TOutput>(
        IParser<TInput, TOutput> p,
        IParser<TInput>? separator = null,
        int minimum = 0,
        int? maximum = null
    ) => new Repetition<TInput>.Parser<TOutput>(p, separator ?? Empty(), minimum, maximum);

    /// <summary>
    /// Parse a list of items with separator in between and defined minimum and maximum quantities.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="separator"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> List(
        IParser<TInput> p,
        IParser<TInput>? separator = null,
        int minimum = 0,
        int? maximum = null
    ) => new Repetition<TInput>.Parser(p, separator ?? Empty(), minimum, maximum);

    /// <summary>
    /// Parse a list of items non-greedily. Will only attempt to match another item if the
    /// continuation parser does not match.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="item"></param>
    /// <param name="separator"></param>
    /// <param name="getContinuation"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> NonGreedyList<TMiddle, TOutput>(
        IParser<TInput, TMiddle> item,
        IParser<TInput> separator,
        GetParserFromParser<TInput, IReadOnlyList<TMiddle>, TOutput> getContinuation,
        int minimum = 0,
        int? maximum = null
    ) => new NonGreedyListParser<TInput, TMiddle, TOutput>(item, separator, getContinuation, minimum, maximum);

    /// <summary>
    /// Parse a list of items non-greedily. Will only attempt to match another item if the
    /// continuation parser does not match.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="item"></param>
    /// <param name="getContinuation"></param>
    /// <param name="separator"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> NonGreedyList<TMiddle, TOutput>(
        IParser<TInput, TMiddle> item,
        GetParserFromParser<TInput, IReadOnlyList<TMiddle>, TOutput> getContinuation,
        IParser<TInput>? separator = null,
        int minimum = 0,
        int? maximum = null
    ) => new NonGreedyListParser<TInput, TMiddle, TOutput>(item, separator ?? Empty(), getContinuation, minimum, maximum);
}
