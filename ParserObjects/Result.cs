using System;
using System.Runtime.CompilerServices;
using ParserObjects.Internal;

namespace ParserObjects;

public static class Result
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Ok<T>(IParser parser, T value, int consumed, ResultData data = default)
        => new Result<T>(parser, true, string.Empty, value, consumed, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Fail<T>(IParser parser, string errorMessage, ResultData data = default)
        => new Result<T>(parser, false, errorMessage ?? string.Empty, default, 0, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TOutput> Fail<TInput, TOutput>(IParser<TInput, TOutput> parser, string errorMessage, ResultData data = default)
        => new Result<TOutput>(parser, false, errorMessage ?? string.Empty, default, 0, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TOutput> Create<TOutput>(IParser parser, PartialResult<TOutput> part)
        => part.ToResult(parser);
}

public readonly record struct Result<TValue>(
    IParser Parser,
    bool Success,
    string? InternalError,
    TValue? InternalValue,
    int Consumed,
    ResultData Data
)
{
    public string ErrorMessage
        => Success
        ? string.Empty
        : InternalError ?? string.Empty;

    public TValue Value
        => Success
        ? InternalValue!
        : throw new InvalidOperationException("This result has failed. There is no value to access: " + ErrorMessage);

    public bool IsValid => (Success && InternalValue != null) || (!Success && !string.IsNullOrEmpty(InternalError));

    /// <summary>
    /// Safely get the value of the result, or the default value.
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public TValue GetValueOrDefault(TValue defaultValue)
        => Success ? Value : defaultValue;

    /// <summary>
    /// Safely get the value of the result, or the default value.
    /// </summary>
    /// <param name="getDefaultValue"></param>
    /// <returns></returns>
    public TValue GetValueOrDefault(Func<TValue> getDefaultValue)
        => Success ? Value : getDefaultValue();

    public Option<TValue> ToOption()
        => Match(static v => new Option<TValue>(true, v), static () => default);

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TResult> onFailure)
        => Success
        ? onSuccess(Value)
        : onFailure();

    public Result<T> Select<T>(Func<TValue, T> selector)
        => Success
        ? new Result<T>(Parser, true, null, selector(Value), Consumed, Data)
        : new Result<T>(Parser, false, ErrorMessage, default, 0, Data);

    public Result<T> Select<T, TData>(TData data, Func<TValue, TData, T> selector)
        => Success
        ? new Result<T>(Parser, true, null, selector(Value, data), Consumed, Data)
        : new Result<T>(Parser, false, ErrorMessage, default, 0, Data);

    public Result<object> AsObject() => Select<object>(static t => t!);

    public override string ToString()
        => Success
            ? $"{Parser} Ok"
            : $"{Parser} FAIL: {ErrorMessage}";
}

/// <summary>
/// A factory for creating Result objects in the current parser context.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public readonly struct ResultFactory<TInput, TOutput>
{
    private readonly IParser _parser;
    private readonly IParseState<TInput> _state;
    private readonly SequenceCheckpoint _startCheckpoint;

    public ResultFactory(IParser parser, IParseState<TInput> state, SequenceCheckpoint startCheckpoint)
    {
        _parser = parser;
        _state = state;
        _startCheckpoint = startCheckpoint;
    }

    /// <summary>
    /// Create a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="parser"></param>
    /// <returns></returns>
    public Result<TOutput> Failure(string errorMessage, IParser? parser = null)
        => Result.Fail<TOutput>(parser ?? _parser, errorMessage, default);

    /// <summary>
    /// Create a success result with a value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Result<TOutput> Success(TOutput value)
        => Result.Ok(_parser, value, _state.Input.Consumed - _startCheckpoint.Consumed);
}
