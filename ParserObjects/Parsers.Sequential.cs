using System;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Execute a specially-structured callback to turn a parse into sequential, procedural
    /// code.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Sequential<TOutput>(Func<SequentialState<TInput>, TOutput> func)
        => new Sequential.Parser<TInput, TOutput, Func<SequentialState<TInput>, TOutput>>(func, static (s, d) => d(s));

    /// <summary>
    /// Execute a specially-structured callback to turn a parse into sequential, procedural code.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Sequential<TOutput, TData>(TData data, Func<SequentialState<TInput>, TData, TOutput> func)
        => new Sequential.Parser<TInput, TOutput, TData>(data, func);
}

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
            throw new Internal.Parsers.Sequential.ParseFailedException(result.AsObject(), errorMessage);
        return result.Value;
    }

    /// <summary>
    /// Attempt to invoke the parser. Return a result indicating success or failure.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public Result<TOutput> TryParse<TOutput>(IParser<TInput, TOutput> p)
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

    public void Expect(TInput c)
    {
        var real = _state.Input.GetNext();
        if (real?.Equals(c) != true)
            Fail($"Expected {c} but found {real}");
    }

    /// <summary>
    /// Attempt to invoke the parser but consume no input. Returns the result of the parser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <returns></returns>
    public Result<TOutput> TestParse<TOutput>(IParser<TInput, TOutput> p)
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
    [DoesNotReturn]
    public void Fail(string error = "Fail") => throw new Internal.Parsers.Sequential.ParseFailedException(error);

    /// <summary>
    /// Get an array of all inputs consumed by the Sequential so far.
    /// </summary>
    /// <returns></returns>
    public TInput[] GetCapturedInputs()
    {
        var currentCp = _state.Input.Checkpoint();
        return _state.Input.GetArrayBetween(_startCheckpoint, currentCp);
    }

    public SequenceCheckpoint Checkpoint()
        => _state.Input.Checkpoint();
}
