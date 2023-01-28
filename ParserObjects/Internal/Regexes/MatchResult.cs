using ParserObjects.Regexes;

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
        Match = null;
    }

    public MatchResult(string value, int consumed, Location location, RegexMatch match)
    {
        Success = true;
        Value = value;
        Consumed = consumed;
        Location = location;
        ErrorMessage = default;
        Match = match;
    }

    public bool Success { get; }
    public string? Value { get; }

    public int Consumed { get; }
    public Location Location { get; }

    public string? ErrorMessage { get; }

    public RegexMatch? Match { get; }
}
