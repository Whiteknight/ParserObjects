using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public sealed class CapturingGroupState : IState
{
    private readonly string? _name;

    public CapturingGroupState(int groupNumber, List<IState> group)
    {
        GroupNumber = groupNumber;
        Group = group;
    }

    private CapturingGroupState(string name, int groupNumber, List<IState> group)
    {
        _name = name;
        GroupNumber = groupNumber;
        Group = group;
    }

    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name => _name ?? $"Group {GroupNumber}";

    /// <summary>
    /// Gets or sets all substates if this state is a group.
    /// </summary>
    public List<IState> Group { get; set; }

    public int GroupNumber { get; set; }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
         => new CapturingGroupState(name, GroupNumber, Group)
         {
             Quantifier = Quantifier,
             Maximum = Maximum
         };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        var match = test(context.Captures, Group, context.Input);
        if (!match)
            return false;

        var afterMatch = context.Input.Checkpoint();
        var value = GetCaptureString(context.Input, beforeMatch, afterMatch);
        context.Captures.AddCapture(GroupNumber, value);
        return match;
    }

    private static string GetCaptureString(ISequence<char> input, SequenceCheckpoint beforeMatch, SequenceCheckpoint afterMatch)
    {
        if (input is ICharSequence charSequence)
            return charSequence.GetStringBetween(beforeMatch, afterMatch);
        return new string(input.GetBetween(beforeMatch, afterMatch));
    }
}
