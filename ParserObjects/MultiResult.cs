using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal;

namespace ParserObjects;

public readonly record struct ResultAlternative<TOutput>(
    bool Success,
    string? InternalError,
    TOutput? InternalValue,
    int Consumed,
    SequenceCheckpoint Continuation)
{
    public TOutput Value
        => Success
        ? InternalValue!
        : throw new InvalidOperationException("This result has failed. There is no value to access: " + ErrorMessage);

    public string ErrorMessage
        => Success
        ? string.Empty
        : InternalError ?? string.Empty;

    public static ResultAlternative<TOutput> Failure(string errorMessage, SequenceCheckpoint startCheckpoint)
        => new ResultAlternative<TOutput>(false, errorMessage, default!, 0, startCheckpoint);

    public static ResultAlternative<TOutput> Ok(TOutput value, int consumed, SequenceCheckpoint continuation)
        => new ResultAlternative<TOutput>(true, string.Empty, value, consumed, continuation);

    public ResultAlternative<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform)
        => Success
        ? new ResultAlternative<TValue>(true, string.Empty, transform(data, Value), Consumed, Continuation)
        : new ResultAlternative<TValue>(false, ErrorMessage, default!, Consumed, Continuation);
}

public readonly record struct MultiResult<TOutput>(
    IParser Parser,
    SequenceCheckpoint StartCheckpoint,
    IReadOnlyList<ResultAlternative<TOutput>> Results,
    ResultData Data = default
)
{
    public bool Success => Results.Any(r => r.Success);

    public static MultiResult<TOutput> FromSingleFailure(IParser parser, SequenceCheckpoint startCheckpoint, string errorMessage)
        => new MultiResult<TOutput>(parser, startCheckpoint, new[] { ResultAlternative<TOutput>.Failure(errorMessage, startCheckpoint) });

    public MultiResult<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform)
    {
        Assert.ArgumentNotNull(transform);
        return new MultiResult<TValue>(
            Parser,
            StartCheckpoint,
            Results.Select(r => r.Transform(data, transform)).ToList(),
            Data);
    }

    public Result<TOutput> ToResult(ResultAlternative<TOutput> alt)
        => alt.Success
        ? Result<TOutput>.Ok(Parser, alt.Value, alt.Consumed)
        : Result<TOutput>.Fail(Parser, alt.ErrorMessage);

    public MultiResult<object> AsObject() => Transform<object, object?>(null, static (_, x) => x!);

    public MultiResult<TValue> SelectMany<TValue>(Func<ResultAlternative<TOutput>, ResultAlternative<TValue>> select)
    {
        Assert.ArgumentNotNull(select);
        return new MultiResult<TValue>(
            Parser,
            StartCheckpoint,
            Results.Select(r => r.Success ? select(r) : r.Transform<TValue, object?>(null, static (_, _) => default!)).ToList(),
            Data);
    }
}
