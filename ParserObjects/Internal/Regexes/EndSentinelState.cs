﻿using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

/// <summary>
/// End sentinel state is added to the list of states so the engine knows when to stop matching.
/// </summary>
public sealed class EndSentinelState : IState
{
    public Quantifier Quantifier
    {
        get => Quantifier.ExactlyOne;
        set => throw new RegexException("Cannot quantify the End Sentinel state");
    }

    public int Maximum
    {
        get => 0;
        set => throw new RegexException("Cannot quantify the End Sentinel state");
    }

    public string Name => "End Sentinel";

    public INamed SetName(string name) => throw new RegexException("Cannot clone the EndSentinel state");

    public IState Clone() => throw new RegexException("Cannot clone the EndSentinel state");

    public override string ToString() => Name;

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
        => throw new RegexException("Unsupported state type during match");
}
