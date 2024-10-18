using System;

namespace ParserObjects;

public static partial class MultiParserExtensions
{
    /// <summary>
    /// Expect the IMultiResult to contain exactly 1 alternative, and select that to continue.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Single<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
        => Parsers<TInput>.SingleResult(multiParser);

    /// <summary>
    /// Select the result alternative which consumed the most amount of input and use that to
    /// continue the parse. If there are no alternatives, returns failure. If there are ties,
    /// the first is selected.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Longest<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
        => Parsers<TInput>.LongestResult(multiParser);

    /// <summary>
    /// Returns the first successful alternative which matches a predicate to continue the
    /// parse with.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(
        this IMultiParser<TInput, TOutput> multiParser,
        Func<IResultAlternative<TOutput>, bool> predicate
    ) => Parsers<TInput>.FirstResult(multiParser, predicate);

    /// <summary>
    /// Selects the first successful alternative to continue the parse with.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this IMultiParser<TInput, TOutput> multiParser)
        => Parsers<TInput>.FirstResult(multiParser, r => r.Success);

    /// <summary>
    /// Invoke a special callback to attempt to select a single alternative and turn it into
    /// an IResult.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <param name="select"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Select<TInput, TOutput>(
        this IMultiParser<TInput, TOutput> multiParser,
        Func<SelectArguments<TOutput>, Option<IResultAlternative<TOutput>>> select
    ) => Parsers<TInput>.SelectResult(multiParser, select);
}
