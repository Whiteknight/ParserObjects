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
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2) parsers
    );

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2, IParser<TI, TO> P3) parsers
    );

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2, IParser<TI, TO> P3, IParser<TI, TO> P4) parsers
    );

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2, IParser<TI, TO> P3, IParser<TI, TO> P4, IParser<TI, TO> P5) parsers
    );

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2, IParser<TI, TO> P3, IParser<TI, TO> P4, IParser<TI, TO> P5, IParser<TI, TO> P6) parsers
    );

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2, IParser<TI, TO> P3, IParser<TI, TO> P4, IParser<TI, TO> P5, IParser<TI, TO> P6, IParser<TI, TO> P7) parsers
    );

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2, IParser<TI, TO> P3, IParser<TI, TO> P4, IParser<TI, TO> P5, IParser<TI, TO> P6, IParser<TI, TO> P7, IParser<TI, TO> P8) parsers
    );

    /// <summary>
    /// Attempt each parser in order, and return the first successful result, if any.
    /// </summary>
    /// <typeparam name="TI"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="parsers"></param>
    /// <returns></returns>
    public static partial IParser<TI, TO> First<TI, TO>(
        this (IParser<TI, TO> P1, IParser<TI, TO> P2, IParser<TI, TO> P3, IParser<TI, TO> P4, IParser<TI, TO> P5, IParser<TI, TO> P6, IParser<TI, TO> P7, IParser<TI, TO> P8, IParser<TI, TO> P9) parsers
    );
}
