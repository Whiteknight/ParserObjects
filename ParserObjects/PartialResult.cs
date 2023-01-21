using System;

namespace ParserObjects;

/// <summary>
/// A struct representing a partial parse result. Can be converted into an IResult by adding
/// additional information. Used internally to pass values without having to allocate IResult
/// objects on the heap.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public struct PartialResult<TValue>
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

    public TResult Match<TResult>(TResult nothing, Func<TValue, TResult> success)
    {
        if (!Success)
            return nothing;
        return success(Value!);
    }
}
