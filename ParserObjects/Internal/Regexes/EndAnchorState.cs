using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

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

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string _)
        => throw new RegexException("Cannot clone the EndAnchor state");

    public override string ToString() => Name;

    public bool Match(RegexContext context, ISequenceCheckpoint checkpoint, TestFunc test)
        => context.Input.IsAtEnd;
}
