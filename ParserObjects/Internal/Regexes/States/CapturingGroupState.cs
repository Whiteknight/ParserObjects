using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

public sealed class CapturingGroupState : IState
{
    public CapturingGroupState(int groupNumber, List<IState> group)
    {
        GroupNumber = groupNumber;
        Group = group;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public List<IState> Group { get; set; }

    public int GroupNumber { get; set; }

    public IState Clone() => new CapturingGroupState(GroupNumber, Group)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} Group {GroupNumber}";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        var match = test(context.Captures, Group, context, true);
        if (!match)
            return false;

        var afterMatch = context.Input.Checkpoint();
        var value = GetCaptureString(context.Input, beforeMatch, afterMatch);
        context.Captures.AddCapture(GroupNumber, value);
        return match;
    }

    private static string GetCaptureString(ISequence<char> input, SequenceCheckpoint beforeMatch, SequenceCheckpoint afterMatch)
        => input.GetStringBetween(beforeMatch, afterMatch);
}
