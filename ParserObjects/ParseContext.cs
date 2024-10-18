namespace ParserObjects;

/// <summary>
/// Current contextual state of the parse
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <param name="Parser"></param>
/// <param name="State"></param>
/// <param name="Result"></param>
public readonly record struct ParseContext<TInput>(
    IParser<TInput> Parser,
    IParseState<TInput> State,
    IResult? Result
)
{
    /// <summary>
    /// Gets the parse state data.
    /// </summary>
    public DataStore Data => State.Data;

    /// <summary>
    /// Gets the current input sequence.
    /// </summary>
    public ISequence<TInput> Input => State.Input;
}

/// <summary>
/// Current contextual state of the parse
/// </summary>
public readonly record struct ParseContext<TInput, TOutput>(
    IParser<TInput, TOutput> Parser,
    IParseState<TInput> State,
    IResult<TOutput>? Result
)
{
    /// <summary>
    /// Gets the parse state data.
    /// </summary>
    public DataStore Data => State.Data;

    /// <summary>
    /// Gets the current input sequence.
    /// </summary>
    public ISequence<TInput> Input => State.Input;
}

/// <summary>
/// Current contextual state of the parse
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="Parser"></param>
/// <param name="State"></param>
/// <param name="Result"></param>
public readonly record struct MultiParseContext<TInput, TOutput>(
    IMultiParser<TInput, TOutput> Parser,
    IParseState<TInput> State,
    IMultiResult<TOutput>? Result
)
{
    /// <summary>
    /// Gets the parse state data.
    /// </summary>
    public DataStore Data => State.Data;

    /// <summary>
    /// Gets the current input sequence.
    /// </summary>
    public ISequence<TInput> Input => State.Input;
}
