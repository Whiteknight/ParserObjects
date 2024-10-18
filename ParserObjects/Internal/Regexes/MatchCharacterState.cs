namespace ParserObjects.Internal.Regexes;

public sealed class MatchCharacterState : IState
{
    private readonly char _c;
    private readonly string? _name;

    public MatchCharacterState(char c)
    {
        _c = c;
    }

    public MatchCharacterState(string name, char c)
    {
        _name = name;
        _c = c;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public string Name => _name ?? $"Match '{_c}'";

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
        => new MatchCharacterState(name, _c)
        {
            Quantifier = Quantifier,
            Maximum = Maximum
        };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

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
