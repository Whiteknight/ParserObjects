﻿namespace ParserObjects.Pratt
{
    /// <summary>
    /// Calculate the Null Denominator of the given token.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public delegate TOutput NudFunc<TInput, TValue, TOutput>(IParseContext<TInput, TOutput> context, IToken<TValue> value);

    /// <summary>
    /// Calculate the Left Denominator for the given token.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="context"></param>
    /// <param name="left"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public delegate TOutput LedFunc<TInput, TValue, TOutput>(IParseContext<TInput, TOutput> context, IToken<TOutput> left, IToken<TValue> value);
}
