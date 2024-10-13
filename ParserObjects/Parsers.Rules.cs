using System;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        Func<T1, T2, TOutput> produce
    );

    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, T3, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        IParser<TInput, T3> p3,
        Func<T1, T2, T3, TOutput> produce
    );

    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, T3, T4, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        IParser<TInput, T3> p3,
        IParser<TInput, T4> p4,
        Func<T1, T2, T3, T4, TOutput> produce
    );

    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="p5"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, T3, T4, T5, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        IParser<TInput, T3> p3,
        IParser<TInput, T4> p4,
        IParser<TInput, T5> p5,
        Func<T1, T2, T3, T4, T5, TOutput> produce
    );

    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="p5"></param>
    /// <param name="p6"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, T3, T4, T5, T6, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        IParser<TInput, T3> p3,
        IParser<TInput, T4> p4,
        IParser<TInput, T5> p5,
        IParser<TInput, T6> p6,
        Func<T1, T2, T3, T4, T5, T6, TOutput> produce
    );

    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="p5"></param>
    /// <param name="p6"></param>
    /// <param name="p7"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, T3, T4, T5, T6, T7, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        IParser<TInput, T3> p3,
        IParser<TInput, T4> p4,
        IParser<TInput, T5> p5,
        IParser<TInput, T6> p6,
        IParser<TInput, T7> p7,
        Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce
    );

    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="p5"></param>
    /// <param name="p6"></param>
    /// <param name="p7"></param>
    /// <param name="p8"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        IParser<TInput, T3> p3,
        IParser<TInput, T4> p4,
        IParser<TInput, T5> p5,
        IParser<TInput, T6> p6,
        IParser<TInput, T7> p7,
        IParser<TInput, T8> p8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce
    );

    /// <summary>
    /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
    /// all and return failure.
    /// </summary>
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
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="p5"></param>
    /// <param name="p6"></param>
    /// <param name="p7"></param>
    /// <param name="p8"></param>
    /// <param name="p9"></param>
    /// <param name="produce"></param>
    /// <returns></returns>
    public static partial IParser<TInput, TOutput> Rule<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(
        IParser<TInput, T1> p1,
        IParser<TInput, T2> p2,
        IParser<TInput, T3> p3,
        IParser<TInput, T4> p4,
        IParser<TInput, T5> p5,
        IParser<TInput, T6> p6,
        IParser<TInput, T7> p7,
        IParser<TInput, T8> p8,
        IParser<TInput, T9> p9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce
    );
}
