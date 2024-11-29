using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

public sealed class NonCapturingCloisterState : IState
{
    public NonCapturingCloisterState(List<IState> group)
    {
        Group = group;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public List<IState> Group { get; set; }

    public IState Clone() => new NonCapturingCloisterState(Group)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} non-capturing group";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
        => !context.Input.IsAtEnd
        && test(context.Captures, Group, context, true);
}
