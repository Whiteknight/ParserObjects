using System;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal.Regexes.Execution;
using ParserObjects.Internal.Regexes.Patterns;

namespace ParserObjects.Internal.Regexes.States;

/// <summary>
/// Attempts to match the current input item against a match predicate.
/// </summary>
public sealed class MatchPredicateState : IState
{
    private readonly string _description;

    public MatchPredicateState(string description, Func<char, bool> predicate)
    {
        _description = description;
        ValuePredicate = predicate;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public Func<char, bool> ValuePredicate { get; set; }

    public IState Clone() => new MatchPredicateState(_description, ValuePredicate)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {_description}";

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
