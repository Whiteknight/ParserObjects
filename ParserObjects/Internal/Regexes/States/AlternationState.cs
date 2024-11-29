using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

public sealed class AlternationState : IState
{
    public AlternationState(List<List<IState>> alternations)
    {
        Alternations = alternations;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public List<List<IState>> Alternations { get; set; }

    public IState Clone() => new AlternationState(Alternations)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} alternation";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        foreach (var substate in Alternations)
        {
            var matches = test(context.Captures, substate, context, true);
            if (matches)
                return true;
        }

        return false;
    }
}
