using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ParserObjects.Internal.Regexes.States;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes.Patterns;

public static class State
{
    public static IState EndAnchor { get; } = new EndAnchorState();

    public static IState EndSentinel { get; } = new EndSentinelState();

    public static IState Fence { get; } = new FenceState();

    public static string QuantifierToString(Quantifier quantifier, int maximum)
        => quantifier == Quantifier.Range
        ? $"0-{maximum}"
        : quantifier.ToString();
}

public readonly record struct StateList(List<IState> States)
{
    public static StateList Create() => new StateList(new List<IState>());

    public int Count => States.Count;

    public StateList AddMatch(Func<char, bool> predicate, string description)
    {
        VerifyPreviousStateIsNotEndAnchor();
        return Add(CreateMatchState(predicate, description));
    }

    public StateList AddMatch(char c)
    {
        VerifyPreviousStateIsNotEndAnchor();
        return Add(new MatchCharacterState(c));
    }

    public StateList AddMatch(bool invert, IReadOnlyList<(char, char)> ranges)
    {
        VerifyPreviousStateIsNotEndAnchor();
        return Add(new MatchCharacterClassState("character class", invert, ranges));
    }

    public StateList AddEndAnchor()
    {
        VerifyPreviousStateIsNotEndAnchor();
        return Add(State.EndAnchor);
    }

    public StateList AddParserRecurse(IParser<char> parser)
    {
        VerifyPreviousStateIsNotEndAnchor();
        return Add(new ParserState(parser));
    }

    public StateList SetPreviousStateRange(uint min, uint max)
    {
        if (min > max)
            throw new RegexException($"Invalid range. Minimum {min} must be smaller or equal to maximum {max}");
        if (max == 0)
            throw new RegexException("Invalid range. Maximum must be 1 or more");

        Debug.Assert(States.Count >= 1, "Parser should not allow us to quantify without an atom");
        var previousState = States[^1];
        if (previousState.Quantifier != Quantifier.ExactlyOne)
            throw new RegexException("Range quantifier may only follow an unquantified atom");
        if (previousState is EndAnchorState || previousState is FenceState)
            throw new RegexException("Range quantifier may not attach to End or Fence atoms");

        // we have an exact number to match, so add multiple references of previousState to
        // the list to satisfy the requirement. Add min-1 because we already have one on the
        // list.
        if (min == max)
        {
            AddClones(previousState, min - 1);
            return Add(State.Fence);
        }

        if (min >= 1)
            // Copy this state Minimum times, since we must have them all. We already have one
            // on the list, so we will have minimum+1 copies, and the last one we will apply
            // a range onto
            previousState = AddClones(previousState, min);

        if (max == int.MaxValue)
        {
            previousState.Quantifier = Quantifier.ZeroOrMore;
            return Add(State.Fence);
        }

        previousState.Quantifier = Quantifier.Range;
        previousState.Maximum = (int)(max - min);
        return Add(State.Fence);
    }

    public StateList SetPreviousQuantifier(Quantifier quantifier)
    {
        Debug.Assert(quantifier != Quantifier.Range, "Range quantifier cannot be set with this method");

        var previousState = VerifyPreviousStateIsNotEndAnchor();
        Debug.Assert(previousState != null, "Parser should not allow us to not have a previous state here");
        if (previousState.Quantifier != Quantifier.ExactlyOne || previousState is EndAnchorState || previousState is FenceState)
            throw new RegexException("Quantifier may only follow an unquantified atom");

        previousState.Quantifier = quantifier;
        return Add(State.Fence);
    }

    public StateList AddSpecialMatch(char type)
    {
        VerifyPreviousStateIsNotEndAnchor();

        // If it's one of the special char classes (\d \D \w \W \s \S) do that match. Otherwise
        // it's an escaped char and return a normal match.
        return Add(type switch
        {
            'd' => CreateMatchState(c => char.IsDigit(c), "digit"),
            'D' => CreateMatchState(c => !char.IsDigit(c), "not digit"),
            'w' => CreateMatchState(c => char.IsLetterOrDigit(c) || c == '_', "word"),
            'W' => CreateMatchState(c => char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c), "not word"),
            's' => CreateMatchState(c => char.IsWhiteSpace(c), "whitespace"),
            'S' => CreateMatchState(c => !char.IsWhiteSpace(c), "not whitespace"),
            >= '0' and <= '9' => new MatchBackreferenceState(type - '0'),
            _ => new MatchCharacterState(type)
        });
    }

    public StateList AddCapturingGroupState(StateList group)
    {
        VerifyPreviousStateIsNotEndAnchor();
        return Add(new CapturingGroupState(States.Count + 1, group.States));
    }

    public StateList AddNonCapturingCloisterState(StateList group)
    {
        VerifyPreviousStateIsNotEndAnchor();

        // Minor optimization. Since we're not capturing, a cloister of 1 is the same as the inner
        // state with no cloister. /(a)/ is the same as /a/.
        return Add(group.Count == 1
            ? group.States[0]
            : new NonCapturingCloisterState(group.States));
    }

    public StateList AddLookaheadState(bool positive, StateList group)
    {
        VerifyPreviousStateIsNotEndAnchor();
        return Add(new ZeroWidthLookaheadState(positive, group.States));
    }

    public IState? VerifyPreviousStateIsNotEndAnchor()
    {
        if (States.Count == 0)
            return null;
        var lastState = States[^1];
        if (lastState is EndAnchorState)
            throw new RegexException("Cannot add more states, or quantifiers, to end anchor$");
        return lastState;
    }

    private IState AddClones(IState previousState, uint numberOfClones)
    {
        for (int i = 0; i < numberOfClones; i++)
            States.Add(previousState.Clone());
        return States[^1];
    }

    private static MatchPredicateState CreateMatchState(Func<char, bool> predicate, string description)
         => new MatchPredicateState(description, predicate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private StateList Add(IState state)
    {
        States.Add(state);
        return this;
    }
}
