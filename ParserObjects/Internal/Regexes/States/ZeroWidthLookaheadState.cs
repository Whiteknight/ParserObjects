using System.Collections.Generic;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes.States;

internal class ZeroWidthLookaheadState : IState
{
    private readonly string? _name;
    private readonly bool _positive;

    public ZeroWidthLookaheadState(bool positive, List<IState> group)
    {
        _positive = positive;
        Group = group;
    }

    private ZeroWidthLookaheadState(string name, bool positive, List<IState> group)
    {
        _name = name;
        _positive = positive;
        Group = group;
    }

    public Quantifier Quantifier
    {
        get => Quantifier.ExactlyOne;
        set => throw new RegexException("Cannot quantify an atom which has already been quantified");
    }

    public int Maximum
    {
        get => 0;
        set => throw new RegexException("Cannot quantify an atom which has already been quantified");
    }

    public string Name => _name ?? $"{(_positive ? "Positive" : "Negative")} lookahead";

    public List<IState> Group { get; set; }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private ZeroWidthLookaheadState Clone(string name)
         => new ZeroWidthLookaheadState(name, _positive, Group);

    public override string ToString() => Name;

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        var isMatch = test(context.Captures, Group, context, false);
        beforeMatch.Rewind();
        return _positive ? isMatch : !isMatch;
    }
}
