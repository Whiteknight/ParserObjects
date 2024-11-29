using System.Collections.Generic;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes.States;

internal class ZeroWidthLookaheadState : IState
{
    private readonly bool _positive;

    public ZeroWidthLookaheadState(bool positive, List<IState> group)
    {
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

    public List<IState> Group { get; set; }

    public IState Clone() => throw new RegexException("Cannot clone Zero-Width lookahead states");

    public override string ToString() => $"{(_positive ? "Positive" : "Negative")} lookahead";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        var isMatch = test(context.Captures, Group, context, false);
        beforeMatch.Rewind();
        return _positive ? isMatch : !isMatch;
    }
}
