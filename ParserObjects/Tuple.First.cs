using ParserObjects.Parsers;

namespace ParserObjects;

public static partial class TupleExtensions
{
    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2);
    }

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3);
    }

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4);
    }

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5);
    }

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6);
    }

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7);
    }

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8);
    }

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
    {
        return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8, parsers.Item9);
    }
}
