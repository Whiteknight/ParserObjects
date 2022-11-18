using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public sealed class NonCapturingCloisterState : IState
{
    private readonly string? _name;

    public NonCapturingCloisterState(List<IState> group)
    {
        Group = group;
    }

    private NonCapturingCloisterState(string name, List<IState> group)
    {
        _name = name;
        Group = group;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public string Name => _name ?? "Non-capturing group";

    public List<IState> Group { get; set; }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
         => new NonCapturingCloisterState(name, Group)
         {
             Quantifier = Quantifier,
             Maximum = Maximum
         };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public bool Match(RegexContext context, ISequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        return test(context.Captures, Group, context.Input);
    }
}
