using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

/// <summary>
/// Matches the most recent value of a previous capture group.
/// </summary>
public sealed class MatchBackreferenceState : IState
{
    private readonly string? _name;

    public MatchBackreferenceState(int groupNumber)
    {
        GroupNumber = groupNumber;
    }

    private MatchBackreferenceState(string name, int groupNumber)
    {
        _name = name;
        GroupNumber = groupNumber;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public string Name => _name ?? $"Match \\{GroupNumber}";

    public int GroupNumber { get; set; }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private MatchBackreferenceState Clone(string name)
        => new MatchBackreferenceState(name, GroupNumber)
        {
            Quantifier = Quantifier,
            Maximum = Maximum
        };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        var captureValue = context.Captures.GetLatestValueForGroup(GroupNumber);
        if (captureValue == null)
            return false;

        for (int i = 0; i < captureValue.Length; i++)
        {
            if (captureValue[i] != context.Input.GetNext())
                return false;
        }

        return true;
    }
}
