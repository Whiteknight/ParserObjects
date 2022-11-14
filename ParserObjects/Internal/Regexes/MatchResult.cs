using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public struct MatchResult
{
    public MatchResult(string error, Location location)
    {
        Success = false;
        Location = location;
        ErrorMessage = error;
        Consumed = 0;
        Value = default;
        Captures = null;
    }

    public MatchResult(string value, int consumed, Location location, IReadOnlyList<(int, string)> captures)
    {
        Success = true;
        Value = value;
        Consumed = consumed;
        Location = location;
        ErrorMessage = default;
        Captures = captures;
    }

    public bool Success { get; }
    public string? Value { get; }

    public int Consumed { get; }
    public Location Location { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<(int group, string value)>? Captures { get; }

    //public TResult Match<TResult>(TResult nothing, Func<TValue, TResult> success)
    //{
    //    if (!Success)
    //        return nothing;
    //    return success(Value!);
    //}
}
