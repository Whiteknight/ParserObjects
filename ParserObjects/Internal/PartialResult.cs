using System;

namespace ParserObjects.Internal;

/// <summary>
/// A struct representing a partial parse result. Can be converted into an Result by adding
/// additional information. Used internally to pass values without having to allocate Result
/// objects on the heap.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public readonly struct PartialResult<TValue>
{
    public PartialResult(string error)
    {
        Success = false;
        ErrorMessage = error;
        Consumed = 0;
        Value = default;
    }

    public PartialResult(TValue value, int consumed)
    {
        Success = true;
        Value = value;
        Consumed = consumed;
        ErrorMessage = default;
    }

    public bool Success { get; }

    public TValue? Value { get; }

    public int Consumed { get; }

    public string? ErrorMessage { get; }

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TResult> onFailure)
        => Success
        ? onSuccess(Value!)
        : onFailure();

    public TResult Match<TResult, TData>(TData data, Func<TValue, TData, TResult> onSuccess, Func<TData, TResult> onFailure)
        => Success
        ? onSuccess(Value!, data)
        : onFailure(data);

    public Option<TValue> ToOption()
        => Match(static v => new Option<TValue>(true, v), static () => default);

    public Result<TValue> ToResult(IParser parser)
        => Success
        ? new Result<TValue>(parser, true, null, Value, Consumed, default)
        : new Result<TValue>(parser, false, ErrorMessage, default, 0, default);
}
