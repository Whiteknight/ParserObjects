using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

/// <summary>
/// Matches the most recent value of a previous capture group.
/// </summary>
public sealed class MatchBackreferenceState : IState
{
    public MatchBackreferenceState(int groupNumber)
    {
        GroupNumber = groupNumber;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public int GroupNumber { get; set; }

    public IState Clone() => new MatchBackreferenceState(GroupNumber)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} Match \\{GroupNumber}";

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
