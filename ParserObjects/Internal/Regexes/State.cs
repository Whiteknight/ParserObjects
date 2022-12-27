using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

public static class State
{
    public static IState EndAnchor { get; } = new EndAnchorState();

    public static IState EndSentinel { get; } = new EndSentinelState();

    public static IState Fence { get; } = new FenceState();

    public static string QuantifierToString(Quantifier quantifier, int maximum)
    {
        return quantifier switch
        {
            Quantifier.Range => $"0-{maximum}",
            _ => quantifier.ToString()
        };
    }

    public static List<IState> AddMatch(List<IState>? states, Func<char, bool> predicate, string description)
    {
        states ??= new List<IState>();
        VerifyPreviousStateIsNotEndAnchor(states);

        states.Add(CreateMatchState(predicate, description));
        return states;
    }

    public static List<IState> AddMatch(List<IState>? states, char c)
    {
        states ??= new List<IState>();
        VerifyPreviousStateIsNotEndAnchor(states);

        states.Add(new MatchCharacterState(c));
        return states;
    }

    public static List<IState> AddMatch(List<IState>? states, bool invert, IReadOnlyList<(char, char)> ranges)
    {
        states ??= new List<IState>();
        VerifyPreviousStateIsNotEndAnchor(states);

        states.Add(new MatchCharacterClassState("character class", invert, ranges));
        return states;
    }

    public static List<IState> SetPreviousStateRange(List<IState> states, int min, int max)
    {
        if (min > max)
            throw new RegexException($"Invalid range. Minimum {min} must be smaller or equal to maximum {max}");
        if (min < 0 || max <= 0)
            throw new RegexException("Invalid range. Minimum must be 0 or more, and maximum must be 1 or more");

        var previousState = states.LastOrDefault();
        Debug.Assert(previousState != null, "Parser should not allow us to not have a previous state here");
        if (previousState.Quantifier != Quantifier.ExactlyOne)
            throw new RegexException("Range quantifier may only follow an unquantified atom");
        if (previousState is EndAnchorState || previousState is FenceState)
            throw new RegexException("Range quantifier may not attach to End or Fence atoms");

        // we have an exact number to match, so add multiple references of previousState to
        // the list to satisfy the requirement. Add min-1 because we already have one on the
        // list.
        if (min == max)
        {
            for (int i = 0; i < min - 1; i++)
                states.Add(previousState.Clone());
            states.Add(Fence);
            return states;
        }

        if (min >= 1)
        {
            // Copy this state Minimum times, since we must have them all. We already have one
            // on the list, so we will have minimum+1 copies, and the last one we will apply
            // a range onto
            for (int i = 0; i < min; i++)
                states.Add(previousState.Clone());
            previousState = states.Last();
        }

        if (max == int.MaxValue)
        {
            previousState.Quantifier = Quantifier.ZeroOrMore;
            return states;
        }

        previousState.Quantifier = Quantifier.Range;
        previousState.Maximum = max - min;
        return states;
    }

    public static List<IState> SetPreviousQuantifier(List<IState> states, Quantifier quantifier)
    {
        Debug.Assert(quantifier != Quantifier.Range, "Range quantifier cannot be set with this method");

        var previousState = VerifyPreviousStateIsNotEndAnchor(states);
        Debug.Assert(previousState != null, "Parser should not allow us to not have a previous state here");
        if (previousState.Quantifier != Quantifier.ExactlyOne)
            throw new RegexException("Quantifier may only follow an unquantified atom");

        previousState.Quantifier = quantifier;
        return states;
    }

    public static List<IState> AddSpecialMatch(List<IState>? states, char type)
    {
        states ??= new List<IState>();
        VerifyPreviousStateIsNotEndAnchor(states);

        // If it's one of the special char classes (\d \D \w \W \s \S) do that match. Otherwise
        // it's an escaped char and return a normal match.
        var matchState = type switch
        {
            'd' => CreateMatchState(c => char.IsDigit(c), "digit"),
            'D' => CreateMatchState(c => !char.IsDigit(c), "not digit"),
            'w' => CreateMatchState(c => char.IsLetterOrDigit(c) || c == '_', "word"),
            'W' => CreateMatchState(c => char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c), "not word"),
            's' => CreateMatchState(c => char.IsWhiteSpace(c), "whitespace"),
            'S' => CreateMatchState(c => !char.IsWhiteSpace(c), "not whitespace"),
            >= '0' and <= '9' => new MatchBackreferenceState(type - '0'),
            _ => new MatchCharacterState(type)
        };
        states.Add(matchState);
        return states;
    }

    public static List<IState> AddCapturingGroupState(List<IState>? states, List<IState> group)
    {
        states ??= new List<IState>();
        VerifyPreviousStateIsNotEndAnchor(states);

        int groupNumber = states.Count + 1;
        var groupState = new CapturingGroupState(groupNumber, group);
        states.Add(groupState);
        return states;
    }

    public static List<IState> AddNonCapturingCloisterState(List<IState>? states, List<IState> group)
    {
        states ??= new List<IState>();
        VerifyPreviousStateIsNotEndAnchor(states);

        // Minor optimization. Since we're not capturing, a cloister of 1 is the same as the inner
        // state with no cloister. /(a)/ is the same as /a/.
        if (group.Count == 1)
        {
            states.Add(group[0]);
            return states;
        }

        var groupState = new NonCapturingCloisterState(group);
        states.Add(groupState);
        return states;
    }

    private static IState CreateMatchState(Func<char, bool> predicate, string description)
         => new MatchPredicateState(description, predicate);

    private static IState? VerifyPreviousStateIsNotEndAnchor(List<IState> states)
    {
        var previousState = states.LastOrDefault();
        if (previousState is EndAnchorState)
            throw new RegexException("Cannot add more states, or quantifiers, to end anchor$");
        return previousState;
    }
}
