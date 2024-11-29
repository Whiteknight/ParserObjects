using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes.States;

/// <summary>
/// End anchor state when the user specifies "$" and wants to explicitly match the end of the
/// input sequence.
/// </summary>
public sealed class EndAnchorState : IState
{
    public Quantifier Quantifier
    {
        get => Quantifier.ExactlyOne;
        set => throw new RegexException("Cannot quantify the End Anchor $ state");
    }

    public int Maximum
    {
        get => 0;
        set => throw new RegexException("Cannot quantify the End Anchor $ state");
    }

    public string Name => "End Anchor $";

    public INamed SetName(string name) => throw new RegexException("Cannot clone the EndAnchor state");

    public IState Clone() => throw new RegexException("Cannot clone the EndAnchor state");

    public override string ToString() => Name;

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
        => context.Input.IsAtEnd;
}
