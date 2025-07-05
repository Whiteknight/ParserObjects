using System;
using System.Collections.Generic;
using ParserObjects.Internal;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Chain<TMiddle, TOutput>(
        IParser<TInput, TMiddle> p,
        GetParserFromResult<TInput, TMiddle, TOutput> getNext,
        params IParser[] mentions
    ) => Internal.Parsers.Chain<TInput, TOutput>.Create(p, getNext, static (gn, r) => gn(r), mentions);

    /// <summary>
    /// Executes a parser, and uses the value to determine the next parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> ChainWith<TMiddle, TOutput>(
        IParser<TInput, TMiddle> p,
        Action<ParserPredicateBuilder<TInput, TMiddle, TOutput>> setup
    ) => Internal.Parsers.Chain<TInput, TOutput>.Configure(p, setup);

    /// <summary>
    /// Executes a parser without consuming any input, and uses the value to determine the next
    /// parser to execute.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="getNext"></param>
    /// <param name="mentions"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Choose<TMiddle, TOutput>(
        IParser<TInput, TMiddle> p,
        GetParserFromResult<TInput, TMiddle, TOutput> getNext,
        params IParser[] mentions
    ) => Internal.Parsers.Chain<TInput, TOutput>.Create(None(p), getNext, static (gn, r) => gn(r), mentions);

    /// <summary>
    /// Given the next input lookahead value, select the appropriate parser to use to continue
    /// the parse.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Predict<TOutput>(Action<ParserPredicateBuilder<TInput, TInput, TOutput>> setup)
         => ChainWith(Peek(), setup);
}

/// <summary>
/// Associates each parser with a predicate. Returns the first parser where the predicate returns
/// true.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public readonly record struct ParserPredicateBuilder<TInput, TMiddle, TOutput>(
    List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)> Parsers
)
{
    /// <summary>
    /// Add a new parser to be returned when the predicate condition is satisfied.
    /// </summary>
    /// <param name="equals"></param>
    /// <param name="parser"></param>
    /// <returns></returns>
    public ParserPredicateBuilder<TInput, TMiddle, TOutput> When(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)
    {
        Assert.NotNull(equals);
        Assert.NotNull(parser);
        Parsers.Add((equals, parser));
        return this;
    }
}
