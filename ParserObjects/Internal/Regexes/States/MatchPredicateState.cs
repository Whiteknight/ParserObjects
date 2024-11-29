using System;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

/// <summary>
/// Attempts to match the current input item against a match predicate.
/// </summary>
public sealed class MatchPredicateState : IState
{
    public MatchPredicateState(string name, Func<char, bool> predicate)
    {
        Name = name;
        ValuePredicate = predicate;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public string Name { get; }

    public Func<char, bool> ValuePredicate { get; set; }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private MatchPredicateState Clone(string name)
        => new MatchPredicateState(name, ValuePredicate)
        {
            Quantifier = Quantifier,
            Maximum = Maximum
        };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        if (!ValuePredicate(context.Input.Peek()))
            return false;

        context.Input.GetNext();
        return true;
    }
}
