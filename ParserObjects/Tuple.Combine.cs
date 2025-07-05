using System.Collections.Generic;

namespace ParserObjects;

public static partial class TupleExtensions
{
    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2) parsers
    );

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2, IParser<TInput> P3) parsers
    );

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2, IParser<TInput> P3, IParser<TInput> P4) parsers
    );

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2, IParser<TInput> P3, IParser<TInput> P4, IParser<TInput> P5) parsers
    );

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2, IParser<TInput> P3, IParser<TInput> P4, IParser<TInput> P5, IParser<TInput> P6) parsers
    );

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2, IParser<TInput> P3, IParser<TInput> P4, IParser<TInput> P5, IParser<TInput> P6, IParser<TInput> P7) parsers
    );

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2, IParser<TInput> P3, IParser<TInput> P4, IParser<TInput> P5, IParser<TInput> P6, IParser<TInput> P7, IParser<TInput> P8) parsers
    );

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TInput, IReadOnlyList<object>> Combine<TInput>(
        this (IParser<TInput> P1, IParser<TInput> P2, IParser<TInput> P3, IParser<TInput> P4, IParser<TInput> P5, IParser<TInput> P6, IParser<TInput> P7, IParser<TInput> P8, IParser<TInput> P9) parsers
    );
}
