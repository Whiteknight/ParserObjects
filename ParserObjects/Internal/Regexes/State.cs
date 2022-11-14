using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

/// <summary>
/// Represents a state at a point in evaluating a regular expression. At each point, the
/// regex will attempt to match the current state against the current input.
/// </summary>
public sealed class State : INamed
{
    public State(string name)
    {
        Name = name;
    }

    public static State EndOfInput { get; } = new State("end of input")
    {
        Type = StateType.EndOfInput
    };

    public static State EndSentinel { get; } = new State("end sentinel")
    {
        Type = StateType.EndSentinel
    };

    public static State Fence { get; } = new State("fence")
    {
        Type = StateType.Fence,
        Quantifier = Quantifier.ZeroOrOne
    };

    public static State CreateMatchState(char c)
        => CreateMatchState(x => x == c, $"Matches {c}");

    public static List<State> AddMatch(List<State>? states, Func<char, bool> predicate, string description)
    {
        states ??= new List<State>();
        if (states.LastOrDefault()?.Type == StateType.EndOfInput)
            throw new RegexException("Cannot have atoms after the end anchor $");

        states.Add(CreateMatchState(predicate, description));
        return states;
    }

    private static State CreateMatchState(Func<char, bool> predicate, string description)
         => new State(description)
         {
             Type = StateType.MatchValue,
             ValuePredicate = predicate,
             Quantifier = Quantifier.ExactlyOne,
         };

    public static List<State> SetPreviousStateRange(List<State> states, int min, int max)
    {
        if (min > max)
            throw new RegexException($"Invalid range. Minimum {min} must be smaller or equal to maximum {max}");
        if (min < 0 || max <= 0)
            throw new RegexException("Invalid range. Minimum must be 0 or more, and maximum must be 1 or more");

        var previousState = states.LastOrDefault();
        if (previousState == null)
            throw new RegexException("Range quantifier must appear after a valid atom");
        if (previousState.Quantifier != Quantifier.ExactlyOne)
            throw new RegexException("Range quantifier may only follow an unquantified atom");
        if (previousState.Type == StateType.EndOfInput || previousState.Type == StateType.Fence)
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

    public static List<State> SetPreviousQuantifier(List<State> states, Quantifier quantifier)
    {
        if (quantifier == Quantifier.Range)
            throw new RegexException("Range quantifier must have minimum and maximum values");
        var previousState = states.LastOrDefault();
        if (previousState == null)
            throw new RegexException("Quantifier must appear after a valid atom");
        if (previousState.Quantifier != Quantifier.ExactlyOne)
            throw new RegexException("Quantifier may only follow an unquantified atom");
        previousState.Quantifier = quantifier;
        return states;
    }

    public static List<State> AddSpecialMatch(List<State>? states, char type)
    {
        states ??= new List<State>();
        if (states.LastOrDefault()?.Type == StateType.EndOfInput)
            throw new RegexException("Cannot have atoms after the end anchor $");

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
            (>= '0') and (<= '9') => CreateBackreferenceState((int)(type - '0')),
            _ => CreateMatchState(type)
        };
        states.Add(matchState);
        return states;
    }

    private static State CreateBackreferenceState(int index)
    {
        return new State($"Match backreference {index}")
        {
            Type = StateType.MatchBackreference,
            GroupNumber = index,
        };
    }

    public static List<State> AddCapturingGroupState(List<State>? states, List<State> group)
    {
        states ??= new List<State>();
        if (states.LastOrDefault()?.Type == StateType.EndOfInput)
            throw new RegexException("Cannot have atoms after the end anchor $");

        var groupState = new State("group")
        {
            Type = StateType.CapturingGroup,
            Group = group,
            // Set a GroupNumber to be the current position in the array
            // If this group is cloned due to quantification, the GroupNumber of the clone will
            // be the same. These get renumbered later.
            GroupNumber = states.Count + 1,
            Quantifier = Quantifier.ExactlyOne
        };
        states.Add(groupState);
        return states;
    }

    public static List<State> AddNonCapturingCloisterState(List<State>? states, List<State> group)
    {
        states ??= new List<State>();
        if (states.LastOrDefault()?.Type == StateType.EndOfInput)
            throw new RegexException("Cannot have atoms after the end anchor $");

        // Minor optimization. Since we're not capturing, a cloister of 1 is the same as the inner
        // state with no cloister
        if (group.Count == 1)
        {
            states.Add(group[0]);
            return states;
        }

        var groupState = new State("group")
        {
            Type = StateType.NonCapturingCloister,
            Group = group,
            // Set a GroupNumber to be the current position in the array
            // If this group is cloned due to quantification, the GroupNumber of the clone will
            // be the same. These get renumbered later.
            GroupNumber = states.Count + 1,
            Quantifier = Quantifier.ExactlyOne
        };
        states.Add(groupState);
        return states;
    }

    public static List<State> QuantifyPrevious(List<State> states, Quantifier quantifier)
    {
        if (states.LastOrDefault()?.Type == StateType.EndOfInput)
            throw new RegexException("Cannot quantify the end anchor $");
        return SetPreviousQuantifier(states, quantifier);
    }

    /// <summary>
    /// Gets or Sets the type of state.
    /// </summary>
    public StateType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not this state supports backtracking.
    /// </summary>
    public bool CanBacktrack { get; set; }

    /// <summary>
    /// Gets or sets the quantifier that controls how many times this state should match.
    /// </summary>
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets a predicate that determines whether a value matches.
    /// </summary>
    public Func<char, bool>? ValuePredicate { get; set; }

    /// <summary>
    /// Gets or sets all substates if this state is a group.
    /// </summary>
    public List<State>? Group { get; set; }

    public int GroupNumber { get; set; }

    /// <summary>
    /// Gets or sets all possibilities in an alternation.
    /// </summary>
    public List<List<State>>? Alternations { get; set; }

    /// <summary>
    /// Gets a brief description of the state, to help with tracing/debugging.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public override string ToString() => $"{Quantifier} {Name}";

    public State Clone() => Clone(Name);

    private State Clone(string name)
    {
        return new State(name)
        {
            Type = Type,
            CanBacktrack = CanBacktrack,
            Quantifier = Quantifier,
            ValuePredicate = ValuePredicate,
            Group = Group,
            GroupNumber = GroupNumber,
            Alternations = Alternations,
            Maximum = Maximum
        };
    }

    public INamed SetName(string name) => Clone(name);
}
