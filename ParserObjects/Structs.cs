using System;

namespace ParserObjects;

/// <summary>
/// A factory for creating IResult objects in the current parser context.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public readonly struct ResultFactory<TInput, TOutput>
{
    private readonly IParseState<TInput> _state;
    private readonly SequenceCheckpoint _startCheckpoint;

    public ResultFactory(IParser<TInput, TOutput> parser, IParseState<TInput> state, SequenceCheckpoint startCheckpoint)
    {
        Parser = parser;
        _state = state;
        _startCheckpoint = startCheckpoint;
    }

    /// <summary>
    /// Gets the current parser.
    /// </summary>
    public IParser<TInput, TOutput> Parser { get; }

    /// <summary>
    /// Create a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public IResult<TOutput> Failure(string errorMessage)
        => _state.Fail(Parser, errorMessage, default);

    /// <summary>
    /// Create a success result with a value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public IResult<TOutput> Success(TOutput value)
        => _state.Success(Parser, value, _state.Input.Consumed - _startCheckpoint.Consumed);
}

/// <summary>
/// Holds arguments for RightApply.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <param name="Left"></param>
/// <param name="Middle"></param>
/// <param name="Right"></param>
public readonly record struct RightApplyArguments<TOutput, TMiddle>(TOutput Left, TMiddle Middle, TOutput Right);

/// <summary>
/// Holds arguments for Select.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <param name="Result"></param>
/// <param name="Success"></param>
/// <param name="Failure"></param>
public readonly record struct SelectArguments<TOutput>(
    IMultiResult<TOutput> Result,
    Func<IResultAlternative<TOutput>, Option<IResultAlternative<TOutput>>> Success,
    Func<Option<IResultAlternative<TOutput>>> Failure
);

/// <summary>
/// State object for a sequential parse. Handles control flow and input sequence
/// management.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public readonly struct SequentialState<TInput>
{
    private readonly IParseState<TInput> _state;
    private readonly SequenceCheckpoint _startCheckpoint;

    public SequentialState(IParseState<TInput> state, SequenceCheckpoint startCheckpoint)
    {
        _state = state;
        _startCheckpoint = startCheckpoint;
    }

    /// <summary>
    /// Gets the contextual state data.
    /// </summary>
    public DataStore Data => _state.Data;

    /// <summary>
    /// Gets the input sequence.
    /// </summary>
    public ISequence<TInput> Input => _state.Input;

    /// <summary>
    /// Invoke the parser. Exit the Sequential if the parse fails.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    /// <exception cref="Internal.Parsers.Sequential.ParseFailedException">Exits the Sequential if the parse fails.</exception>
    public TOutput Parse<TOutput>(IParser<TInput, TOutput> p, string errorMessage = "")
    {
        var result = p.Parse(_state);
        if (!result.Success)
            throw new Internal.Parsers.Sequential.ParseFailedException(result, errorMessage);
        return result.Value;
    }

    /// <summary>
    /// Attempt to invoke the parser. Return a result indicating success or failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public IResult<TOutput> TryParse<TOutput>(IParser<TInput, TOutput> p)
        => p.Parse(_state);

    /// <summary>
    /// Invoke the parser to match. Returns true, and consumes input, if the match succeeds.
    /// Returns false and consumes no input otherwise.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool Match(IParser<TInput> p) => p.Match(_state);

    /// <summary>
    /// Invoke the parser to match. Fails the entire Sequential if the match fails.
    /// </summary>
    /// <param name="p"></param>
    /// <exception cref="Internal.Parsers.Sequential.ParseFailedException">Immediately exits the Sequential.</exception>
    public void Expect(IParser<TInput> p)
    {
        var ok = p.Match(_state);
        if (!ok)
            throw new Internal.Parsers.Sequential.ParseFailedException("Expect failed");
    }

    /// <summary>
    /// Attempt to invoke the parser but consume no input. Returns the result of the parser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public IResult<TOutput> TestParse<TOutput>(IParser<TInput, TOutput> p)
    {
        var checkpoint = _state.Input.Checkpoint();
        var result = p.Parse(_state);
        checkpoint.Rewind();
        return result;
    }

    /// <summary>
    /// Unconditional failure. Exit the Sequential.
    /// </summary>
    /// <param name="error"></param>
    /// <exception cref="Internal.Parsers.Sequential.ParseFailedException">Immediately exits the Sequential.</exception>
    public void Fail(string error = "Fail") => throw new Internal.Parsers.Sequential.ParseFailedException(error);

    /// <summary>
    /// Get an array of all inputs consumed by the Sequential so far.
    /// </summary>
    /// <returns></returns>
    public TInput[] GetCapturedInputs()
    {
        var currentCp = _state.Input.Checkpoint();
        return _state.Input.GetBetween(_startCheckpoint, currentCp);
    }
}
