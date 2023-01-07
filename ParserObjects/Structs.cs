﻿using System.Collections.Generic;

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
    public DataStore Data => State.Data;
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
    public DataStore Data => State.Data;
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
    public DataStore Data => State.Data;
    public ISequence<TInput> Input => State.Input;
}

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
    /// <param name="location"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public IResult<TOutput> Failure(string errorMessage, Location? location = null, IReadOnlyList<object>? data = null)
        => _state.Fail(Parser, errorMessage, location ?? _state.Input.CurrentLocation, data);

    /// <summary>
    /// Create a success result with a value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="location"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public IResult<TOutput> Success(TOutput value, Location? location = null, IReadOnlyList<object>? data = null)
        => _state.Success(Parser, value, _state.Input.Consumed - _startCheckpoint.Consumed, location ?? _state.Input.CurrentLocation, data);
}

public readonly record struct RightApplyArguments<TOutput, TMiddle>(TOutput Left, TMiddle Middle, TOutput Right);
