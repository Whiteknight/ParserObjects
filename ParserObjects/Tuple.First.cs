using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class TupleExtensions
{
    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2]);

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2, parsers.Item3]);

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4]);

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5]);

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6]);

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7]);

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8]);

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>, IParser<TI, TO>) parsers
    ) => new FirstParser<TI>.WithOutput<TO>([parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8, parsers.Item9]);
}
