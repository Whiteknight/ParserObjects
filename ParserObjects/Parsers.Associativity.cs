using System;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// A left-associative parser where the left item is parsed unconditionally, and the result of the
    /// left parser is applied to the right parser. This new result is then treated as the 'left' value
    /// for the next iteration of the right parser. This can be used when many rules have a common prefix
    /// and you don't want to backtrack through the prefix on every attempt.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="left"></param>
    /// <param name="getRight"></param>
    /// <param name="quantifier"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> LeftApply<TOutput>(
        IParser<TInput, TOutput> left,
        GetParserFromParser<TInput, TOutput> getRight,
        Quantifier quantifier = Quantifier.ZeroOrMore
    ) => new LeftApplyParser<TInput, TOutput>(left, getRight, quantifier);

    /// <summary>
    /// A right-associative parser which attempts to parse a sequence of items separated by middles
    /// recursively: self := &lt;item&gt; (&lt;middle&gt; &lt;self&gt;)*.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="item"></param>
    /// <param name="middle"></param>
    /// <param name="produce"></param>
    /// <param name="getMissingRight"></param>
    /// <param name="quantifier"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> RightApply<TMiddle, TOutput>(
        IParser<TInput, TOutput> item,
        IParser<TInput, TMiddle> middle,
        Func<RightApplyArguments<TOutput, TMiddle>, TOutput> produce,
        Func<IParseState<TInput>, TOutput>? getMissingRight = null,
        Quantifier quantifier = Quantifier.ZeroOrMore
    ) => new RightApplyParser<TInput, TMiddle, TOutput>(item, middle, produce, quantifier, getMissingRight);

    /// <summary>
    /// A right-associative parser which attempts to parse a sequence of items separated by middles
    /// recursively: self := &lt;item&gt; (&lt;middle&gt; &lt;self&gt;)*.
    /// </summary>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="item"></param>
    /// <param name="middle"></param>
    /// <param name="produce"></param>
    /// <param name="getMissingRight"></param>
    /// <param name="quantifier"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> RightApply<TMiddle, TOutput>(
        IParser<TInput, TOutput> item,
        IParser<TInput, TMiddle> middle,
        Func<TOutput, TMiddle, TOutput, TOutput> produce,
        Func<IParseState<TInput>, TOutput>? getMissingRight = null,
        Quantifier quantifier = Quantifier.ZeroOrMore
    ) => new RightApplyParser<TInput, TMiddle, TOutput>(
        item,
        middle,
        args => produce(args.Left, args.Middle, args.Right),
        quantifier,
        getMissingRight
    );
}

/// <summary>
/// Holds arguments for RightApply.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <param name="Left"></param>
/// <param name="Middle"></param>
/// <param name="Right"></param>
public readonly record struct RightApplyArguments<TOutput, TMiddle>(TOutput Left, TMiddle Middle, TOutput Right);
