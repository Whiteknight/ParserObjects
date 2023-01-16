using System;

namespace ParserObjects;

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
