using System;
using System.Collections.Generic;

namespace ParserObjects;

public static class ParserStateExtensions
{
    /// <summary>
    /// The result value of the parser is stored as contextual state data in the parse state.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> SetResultData<TInput, TOutput>(this IParser<TInput, TOutput> p, string name)
        => Parsers<TInput>.SetResultData(p, name);

    /// <summary>
    /// The result value of the parser is stored as contextual state data in the parse state.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="p"></param>
    /// <param name="name"></param>
    /// <param name="getValue"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> SetResultData<TInput, TOutput, TValue>(this IParser<TInput, TOutput> p, string name, Func<TOutput, TValue> getValue)
        => Parsers<TInput>.SetResultData(p, name, getValue);

    /// <summary>
    /// Push a recursive data frame before executing the given parser, and then pop the data
    /// frame when the parser completes.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> WithDataContext<TInput, TOutput>(this IParser<TInput, TOutput> p)
        => Parsers<TInput>.DataContext(p);

    /// <summary>
    /// Push a recursive data frame before executing the given parser, and then pop the data
    /// frame when the parser completes.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="p"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IParser<TInput, TOutput> p, string name, TData value)
        where TData : notnull
        => Parsers<TInput>.DataContext(p, name, value);

    /// <summary>
    /// Push a recursive data frame before executing the given parser, and then pop the data
    /// frame when the parser completes.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="p"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IParser<TInput, TOutput> p, Dictionary<string, TData> values)
        => Parsers<TInput>.DataContext(p, values);

    /// <summary>
    /// Push a recursive data frame before executing the given parser, and then pop the data
    /// frame when the parser completes.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> WithDataContext<TInput, TOutput>(this IMultiParser<TInput, TOutput> p)
        => Parsers<TInput>.DataContext(p);

    /// <summary>
    /// Push a recursive data frame before executing the given parser, and then pop the data
    /// frame when the parser completes.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="p"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IMultiParser<TInput, TOutput> p, string name, TData value)
        where TData : notnull
        => Parsers<TInput>.DataContext(p, name, value);

    /// <summary>
    /// Push a recursive data frame before executing the given parser, and then pop the data
    /// frame when the parser completes.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="p"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> WithDataContext<TInput, TOutput, TData>(this IMultiParser<TInput, TOutput> p, Dictionary<string, TData> values)
        => Parsers<TInput>.DataContext(p, values);
}
