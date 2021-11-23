using System.Collections.Generic;
using ParserObjects.Parsers;

namespace ParserObjects;

public static partial class TupleExtensions
{
    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2 }, r => r);
    }

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3 }, r => r);
    }

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4 }, r => r);
    }

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5 }, r => r);
    }

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6 }, r => r);
    }

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7 }, r => r);
    }

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8 }, r => r);
    }

    /// <summary>
    /// Execute the given parsers in order and return an ordered list of result values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, IReadOnlyList<object>> Combine<TInput>(this (IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>, IParser<TInput>) parsers)
    {
        return new RuleParser<TInput, IReadOnlyList<object>>(new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8, parsers.Item9 }, r => r);
    }
}
