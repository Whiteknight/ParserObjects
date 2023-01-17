using System;
using System.Collections.Generic;

namespace ParserObjects;

public static partial class MultiParserExtensions
{
    /// <summary>
    /// Continue the parse with each alternative separately.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ContinueWith<TInput, TMiddle, TOutput>(
        this IMultiParser<TInput, TMiddle> multiParser,
        GetParserFromParser<TInput, TMiddle, TOutput> getParser
    ) => Parsers<TInput>.ContinueWith(multiParser, getParser);

    /// <summary>
    /// Continue the parse with each alternative separately.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <param name="getParser"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ContinueWith<TInput, TMiddle, TOutput>(
        this IMultiParser<TInput, TMiddle> multiParser,
        GetMultiParserFromParser<TInput, TMiddle, TOutput> getParser
    ) => Parsers<TInput>.ContinueWith(multiParser, getParser);

    /// <summary>
    /// Continue the parse with all the given parsers.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="getParsers"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> ContinueWithEach<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> parser, Func<IParser<TInput, TMiddle>, IEnumerable<IParser<TInput, TOutput>>> getParsers)
        => Parsers<TInput>.ContinueWithEach(parser, getParsers);

    /// <summary>
    /// Transform the values of all result alternatives.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="multiParser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IMultiParser<TInput, TMiddle> multiParser, Func<TMiddle, TOutput> transform)
        => Parsers<TInput>.Transform(multiParser, transform);
}
