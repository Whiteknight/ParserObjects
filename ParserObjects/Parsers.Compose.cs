using System;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Compose two parsers together such that the outputs of the inner parser are used as the
    /// inputs to the outer parser.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="inner"></param>
    /// <param name="outer"></param>
    /// <param name="onEnd"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Compose<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> inner, IParser<TMiddle, TOutput> outer, Func<TMiddle>? onEnd = null)
        => new ComposeParser<TInput, TMiddle, TOutput>(inner, outer, null);
}
