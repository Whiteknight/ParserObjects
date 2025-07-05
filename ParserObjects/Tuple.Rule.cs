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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2) parsers,
        Func<T1, T2, TOutput> produce
    );

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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2, IParser<TInput, T3> P3) parsers,
        Func<T1, T2, T3, TOutput> produce
    );

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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2, IParser<TInput, T3> P3, IParser<TInput, T4> P4) parsers,
        Func<T1, T2, T3, T4, TOutput> produce
    );

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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2, IParser<TInput, T3> P3, IParser<TInput, T4> P4, IParser<TInput, T5> P5) parsers,
        Func<T1, T2, T3, T4, T5, TOutput> produce
    );

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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2, IParser<TInput, T3> P3, IParser<TInput, T4> P4, IParser<TInput, T5> P5, IParser<TInput, T6> P6) parsers,
        Func<T1, T2, T3, T4, T5, T6, TOutput> produce
    );

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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2, IParser<TInput, T3> P3, IParser<TInput, T4> P4, IParser<TInput, T5> P5, IParser<TInput, T6> P6, IParser<TInput, T7> P7) parsers,
        Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce
    );

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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2, IParser<TInput, T3> P3, IParser<TInput, T4> P4, IParser<TInput, T5> P5, IParser<TInput, T6> P6, IParser<TInput, T7> P7, IParser<TInput, T8> P8) parsers,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce
    );

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
    public static partial IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(
        this (IParser<TInput, T1> P1, IParser<TInput, T2> P2, IParser<TInput, T3> P3, IParser<TInput, T4> P4, IParser<TInput, T5> P5, IParser<TInput, T6> P6, IParser<TInput, T7> P7, IParser<TInput, T8> P8, IParser<TInput, T9> P9) parsers,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce
    );
}
