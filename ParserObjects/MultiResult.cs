using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal;

namespace ParserObjects;

public sealed class MultResult<TOutput> : IMultResult<TOutput>
{
    private readonly ResultData _data;

    public MultResult(
        IParser parser,
        SequenceCheckpoint startCheckpoint,
        IEnumerable<IResultAlternative<TOutput>> results,
        ResultData data = default
    )
    {
        Parser = parser;
        Results = results.ToList();
        Success = Results.Any(r => r.Success);
        StartCheckpoint = startCheckpoint;
        _data = data;
    }

    public IParser Parser { get; }

    public bool Success { get; }

    public IReadOnlyList<IResultAlternative<TOutput>> Results { get; }

    public SequenceCheckpoint StartCheckpoint { get; }

    IReadOnlyList<IResultAlternative> IMultResult.Results => Results;

    public IMultResult<TOutput> Recreate(
        CreateNewResultAlternative<TOutput> recreate,
        IParser? parser = null,
        SequenceCheckpoint? startCheckpoint = null,
        Location? location = null
    )
    {
        Assert.ArgumentNotNull(recreate);
        var newAlternatives = Results.Select(alt => !alt.Success ? alt : recreate(alt, alt.Factory));
        var newCheckpoint = startCheckpoint ?? StartCheckpoint;
        return new MultResult<TOutput>(Parser, newCheckpoint, newAlternatives);
    }

    public IMultResult<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform)
    {
        Assert.ArgumentNotNull(transform);
        var newAlternatives = new List<IResultAlternative<TValue>>();
        for (int i = 0; i < Results.Count; i++)
        {
            var newAlt = Results[i].Transform(data, transform);
            newAlternatives.Add(newAlt);
        }

        return new MultResult<TValue>(Parser, StartCheckpoint, newAlternatives);
    }

    public Option<T> TryGetData<T>() => _data.TryGetData<T>();
}

/// <summary>
/// Result value which represents a single success, including information necessary to continue
/// the parse from the point of the success.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
public sealed class SuccessResultAlternative<TOutput> : IResultAlternative<TOutput>
{
    public SuccessResultAlternative(TOutput value, int consumed, SequenceCheckpoint continuation)
    {
        Assert.ArgumentNotNull(continuation);
        Value = value;
        Consumed = consumed;
        Continuation = continuation;
    }

    public static IResultAlternative<TOutput> FactoryMethod(TOutput value, int consumed, SequenceCheckpoint continuation)
        => new SuccessResultAlternative<TOutput>(value, consumed, continuation);

    public bool Success => true;

    public string ErrorMessage => string.Empty;

    public TOutput Value { get; }

    public int Consumed { get; }

    public SequenceCheckpoint Continuation { get; }

    public ResultAlternativeFactoryMethod<TOutput> Factory => FactoryMethod;

    object IResultAlternative.Value => Value!;

    public IResultAlternative<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform)
    {
        var newValue = transform(data, Value);
        return new SuccessResultAlternative<TValue>(newValue, Consumed, Continuation);
    }
}

/// <summary>
/// Result value which represents a single failure.
/// </summary>
/// <typeparam name="TOutput"></typeparam>
public sealed class FailureResultAlternative<TOutput> : IResultAlternative<TOutput>
{
    public FailureResultAlternative(string errorMessage, SequenceCheckpoint continuation)
    {
        ErrorMessage = errorMessage;
        Continuation = continuation;
    }

    public bool Success => false;

    public string ErrorMessage { get; }

    public TOutput Value => throw new InvalidOperationException("This result is not a success and does not have a valid value.");

    object IResultAlternative.Value => throw new InvalidOperationException("This result is not a success and does not have a valid value.");

    public int Consumed => 0;

    public SequenceCheckpoint Continuation { get; }

    public ResultAlternativeFactoryMethod<TOutput> Factory => throw new InvalidOperationException("This result is not a success and does not have a factory");

    public IResultAlternative<TValue> Transform<TValue, TData>(TData data, Func<TData, TOutput, TValue> transform)
        => new FailureResultAlternative<TValue>(ErrorMessage, Continuation);
}
