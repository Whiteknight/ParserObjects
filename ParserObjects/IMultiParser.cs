namespace ParserObjects;

/// <summary>
/// A parser which may return multiple possible results.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public interface IMultiParser<in TInput> : IParser
{
    /// <summary>
    /// Parse the input sequence and return an IMultiResult. The parse method may leave the
    /// input sequence in an indeterminate state, so a continuation checkpoint from the result
    /// should be selected before continuing the parse.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    IMultiResult Parse(IParseState<TInput> state);
}

/// <summary>
/// A parser which may return multiple possible typed results.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public interface IMultiParser<in TInput, TOutput> : IMultiParser<TInput>, ISymbol<TOutput>
{
    /// <summary>
    /// Parse an input sequence and return an IMultiResult. The parse method may leave the
    /// input sequence in an indeterminate state, so a continuation checkpoint from the result
    /// should be selected before continuing the parse.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    new IMultiResult<TOutput> Parse(IParseState<TInput> state);
}
