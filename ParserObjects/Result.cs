using System;
using System.Collections.Generic;

namespace ParserObjects;

public readonly record struct ResultData(IReadOnlyList<object>? Data)
{
    public Option<T> TryGetData<T>()
    {
        if (Data == null)
            return default;

        foreach (var item in Data)
        {
            if (item is T typed)
                return new Option<T>(true, typed);
        }

        return default;
    }
}

/// <summary>
/// Result object representing success. This result contains a valid value and metadata about
/// that value.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed record SuccessResult<TValue>(
    IParser Parser,
    TValue Value,
    int Consumed,
    ResultData Data = default
) : IResult<TValue>
{
    public bool Success => true;
    public string ErrorMessage => string.Empty;
    object IResult.Value => Value!;

    public Option<T> TryGetData<T>() => Data.TryGetData<T>();

    public override string ToString() => $"{Parser} Ok";

    public IResult<TValue> AdjustConsumed(int consumed)
    {
        if (Consumed == consumed)
            return this;
        return this with { Consumed = consumed };
    }

    IResult IResult.AdjustConsumed(int consumed) => AdjustConsumed(consumed);
}

/// <summary>
/// Result object representing failure. This result contains an error message and information
/// about the location of the error. Attempting to access the Value will result in an
/// exception.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed record FailureResult<TValue>(
    IParser Parser,
    string ErrorMessage,
    ResultData Data = default
) : IResult<TValue>
{
    public bool Success => false;
    public TValue Value => throw new InvalidOperationException("This result has failed. There is no value to access: " + ErrorMessage);
    public int Consumed => 0;
    object IResult.Value => Value!;

    public Option<T> TryGetData<T>() => Data.TryGetData<T>();

    public override string ToString() => $"{Parser} FAIL: {ErrorMessage}";

    public IResult<TValue> AdjustConsumed(int consumed) => this;

    IResult IResult.AdjustConsumed(int consumed) => this;
}
