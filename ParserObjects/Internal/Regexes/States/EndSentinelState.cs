using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes.States;

/// <summary>
/// End sentinel state is added to the list of states so the engine knows when to stop matching.
/// </summary>
[ExcludeFromCodeCoverage]
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

    public IState Clone() => throw new RegexException("Cannot clone the EndSentinel state");

    public override string ToString() => "End Sentinel";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
        => throw new RegexException("Unsupported state type during match");
}
