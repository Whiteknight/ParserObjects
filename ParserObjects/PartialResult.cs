using System;

namespace ParserObjects;

public struct PartialResult<TValue>
{
    public PartialResult(string error, Location location)
    {
        Success = false;
        Location = location;
        ErrorMessage = error;
        Consumed = 0;
        Value = default;
    }

    public PartialResult(TValue value, int consumed, Location location)
    {
        Success = true;
        Value = value;
        Consumed = consumed;
        Location = location;
        ErrorMessage = default;
    }

    public bool Success { get; }
    public TValue? Value { get; }

    public int Consumed { get; }
    public Location Location { get; }

    public string? ErrorMessage { get; }

    public TResult Match<TResult>(TResult nothing, Func<TValue, TResult> success)
    {
        if (!Success)
            return nothing;
        return success(Value!);
    }
}
