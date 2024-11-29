using System.Collections.Generic;
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

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    /// <summary>
    /// Gets or sets all substates if this state is a group.
    /// </summary>
    public List<IState> Group { get; set; }

    public int GroupNumber { get; set; }

    public IState Clone() => new CapturingGroupState(GroupNumber, Group)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

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
