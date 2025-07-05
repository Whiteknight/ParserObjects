using System;
using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Internal.Assert;

namespace ParserObjects;

public readonly record struct Alternative<TOutput>(
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

    public static Alternative<TOutput> Failure(string errorMessage, SequenceCheckpoint startCheckpoint)
        => new Alternative<TOutput>(false, errorMessage, default!, 0, startCheckpoint);

    public static Alternative<TOutput> Ok(TOutput value, int consumed, SequenceCheckpoint continuation)
        => new Alternative<TOutput>(true, string.Empty, value, consumed, continuation);

    public Alternative<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform)
        => Success
        ? new Alternative<TValue>(true, string.Empty, transform(data, Value), Consumed, Continuation)
        : new Alternative<TValue>(false, ErrorMessage, default!, Consumed, Continuation);
}

public readonly record struct MultiResult<TOutput>(
    IParser Parser,
    IReadOnlyList<Alternative<TOutput>> Results,
    ResultData Data = default
)
{
    public bool Success => Results.Any(r => r.Success);

    public static MultiResult<TOutput> FromSingleFailure(IParser parser, SequenceCheckpoint startCheckpoint, string errorMessage)
        => new MultiResult<TOutput>(parser, new[] { Alternative<TOutput>.Failure(errorMessage, startCheckpoint) });

    public MultiResult<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform)
    {
        NotNull(transform);
        return new MultiResult<TValue>(
            Parser,
            Results.Select(r => r.Transform(data, transform)).ToList(),
            Data);
    }

    public Result<TOutput> ToResult(Alternative<TOutput> alt)
        => alt.Success
        ? Result.Ok(Parser, alt.Value, alt.Consumed)
        : Result.Fail<TOutput>(Parser, alt.ErrorMessage);

    public MultiResult<object> AsObject() => Transform<object, object?>(null, static (_, x) => x!);

    public MultiResult<TValue> SelectMany<TValue>(Func<Alternative<TOutput>, Alternative<TValue>> select)
    {
        NotNull(select);
        return new MultiResult<TValue>(
            Parser,
            Results.Select(r => r.Success ? select(r) : r.Transform<TValue, object?>(null, static (_, _) => default!)).ToList(),
            Data);
    }
}
