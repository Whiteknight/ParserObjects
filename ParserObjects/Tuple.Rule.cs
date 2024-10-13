using System;

namespace ParserObjects;

public static partial class TupleExtensions
{
    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>) parsers,
        Func<T1, T2, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, produce);

    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>) parsers,
        Func<T1, T2, T3, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, parsers.Item3, produce);

    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>) parsers,
        Func<T1, T2, T3, T4, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, produce);

    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>) parsers,
        Func<T1, T2, T3, T4, T5, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, produce);

    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>) parsers,
        Func<T1, T2, T3, T4, T5, T6, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, produce);

    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>, IParser<TInput, T7>) parsers,
        Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, produce);

    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>, IParser<TInput, T7>, IParser<TInput, T8>) parsers,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8, produce);

    /// <summary>
    /// Execute the given parsers in order and use the output results in a production rule to
    /// create an output value.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="T9"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(
        this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>, IParser<TInput, T7>, IParser<TInput, T8>, IParser<TInput, T9>) parsers,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce
    ) => Parsers<TInput>.Rule(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8, parsers.Item9, produce);
}
