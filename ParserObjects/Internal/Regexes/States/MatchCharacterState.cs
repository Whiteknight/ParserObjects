using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

public sealed class MatchCharacterState : IState
{
    private readonly char _c;

    public MatchCharacterState(char c)
    {
        _c = c;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public IState Clone() => new MatchCharacterState(_c)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} Match '{_c}'";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        if (context.Input.Peek() != _c)
            return false;

        context.Input.GetNext();
        return true;
    }
}
