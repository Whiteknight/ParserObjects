using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes.States;

/// <summary>
/// State used to prevent re-applying a second quantifier to an atom which has already been
/// quantified. For example, "a{2}" would otherwise create states [ExactlyOne(a), ExactlyOne(a)],
/// which appears not to be quantified, but it is. So instead we create states
/// [ExactlyOne(a), ExactlyOne(a), Fence], and Fence will throw an exception if the regex attempts
/// to add a quantifier to it.
/// </summary>
public sealed class FenceState : IState
{
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

    public string Name => "Fence";

    public INamed SetName(string name) => throw new RegexException("Cannot clone the Fence state");

    public IState Clone() => throw new RegexException("Cannot clone the Fence state");

    public override string ToString() => Name;

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
        => throw new RegexException("Unsupported state type during match");
}
