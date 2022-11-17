using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Sequences;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

public interface IState : INamed
{
    /// <summary>
    /// Gets or sets the quantifier that controls how many times this state should match.
    /// </summary>
    public Quantifier Quantifier { get; set; }

    int Maximum { get; set; }

    IState Clone();

    (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test);
}

public sealed class EndAnchorState : IState
{
    public Quantifier Quantifier { get; set; }

    public string Name { get; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public EndAnchorState(string name)
    {
        Name = name;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
        => throw new RegexException("Cannot clone the EndAnchor state");

    public override string ToString() => "end anchor";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        return (buffer.IsPastEnd(index), 0);
    }
}

public sealed class CapturingGroupState : IState
{
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    /// <summary>
    /// Gets or sets all substates if this state is a group.
    /// </summary>
    public List<IState> Group { get; set; }

    public int GroupNumber { get; set; }

    public CapturingGroupState(string name, int groupNumber, List<IState> group)
    {
        Name = name;
        GroupNumber = groupNumber;
        Group = group;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
         => new CapturingGroupState(name, GroupNumber, Group)
         {
             Quantifier = Quantifier,
             Maximum = Maximum
         };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        if (buffer.IsPastEnd(index))
            return (false, 0);

        var groupBuffer = buffer.CopyFrom(index);
        var (match, length) = test(context.Captures, Group, groupBuffer);
        if (!match)
            return (false, 0);

        var value = new string(groupBuffer.Extract(0, length));
        context.Captures.AddCapture(GroupNumber, value);
        return (match, length);
    }
}

public sealed class NonCapturingCloisterState : IState
{
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    /// <summary>
    /// Gets or sets all substates if this state is a group.
    /// </summary>
    public List<IState> Group { get; set; }

    public NonCapturingCloisterState(string name, List<IState> group)
    {
        Name = name;
        Group = group;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
         => new NonCapturingCloisterState(name, Group)
         {
             Quantifier = Quantifier,
             Maximum = Maximum
         };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        if (buffer.IsPastEnd(index))
            return (false, 0);

        var groupBuffer = buffer.CopyFrom(index);
        return test(context.Captures, Group, groupBuffer);
    }
}

public sealed class MatchBackreferenceState : IState
{
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    public int GroupNumber { get; set; }

    public MatchBackreferenceState(string name, int groupNumber)
    {
        Name = name;
        GroupNumber = groupNumber;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
        => new MatchBackreferenceState(name, GroupNumber)
        {
            Quantifier = Quantifier,
            Maximum = Maximum
        };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        if (buffer.IsPastEnd(index))
            return (false, 0);

        var captureValue = context.Captures.GetLatestValueForGroup(GroupNumber);
        if (captureValue == null)
            return (false, 0);

        for (int i = 0; i < captureValue.Length; i++)
        {
            if (buffer[index + i] != captureValue[i])
                return (false, 0);
        }

        return (true, captureValue.Length);
    }
}

public sealed class MatchValueState : IState
{
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    /// <summary>
    /// Gets or sets a predicate that determines whether a value matches.
    /// </summary>
    public Func<char, bool> ValuePredicate { get; set; }

    public MatchValueState(string name, Func<char, bool> predicate)
    {
        Name = name;
        ValuePredicate = predicate;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
        => new MatchValueState(name, ValuePredicate)
        {
            Quantifier = Quantifier,
            Maximum = Maximum
        };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        if (buffer.IsPastEnd(index))
            return (false, 0);

        var match = ValuePredicate(buffer[index]);
        return (match, match ? 1 : 0);
    }
}

public sealed class AlternationState : IState
{
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    /// <summary>
    /// Gets or sets all possibilities in an alternation.
    /// </summary>
    public List<List<IState>> Alternations { get; set; }

    public AlternationState(string name, List<List<IState>> alternations)
    {
        Name = name;
        Alternations = alternations;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
    => new AlternationState(name, Alternations)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        if (buffer.IsPastEnd(index))
            return (false, 0);

        foreach (var substate in Alternations)
        {
            var (matches, consumed) = test(context.Captures, substate, buffer.CopyFrom(index));
            if (matches)
                return (true, consumed);
        }

        return (false, 0);
    }
}

public sealed class EndSentinelState : IState
{
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    public EndSentinelState(string name)
    {
        Name = name;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
        => throw new RegexException("Cannot clone the EndSentinel state");

    public override string ToString() => "end sentinel";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        throw new RegexException("Unsupported state type during match");
    }
}

public sealed class FenceState : IState
{
    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    public FenceState(string name)
    {
        Name = name;
    }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private IState Clone(string name)
        => throw new RegexException("Cannot clone the Fence state");

    public override string ToString() => "fence";

    public (bool matches, int length) Match(RegexContext context, SequenceBuffer<char> buffer, int index, TestFunc test)
    {
        throw new RegexException("Unsupported state type during match");
    }
}

/// <summary>
/// Represents a state at a point in evaluating a regular expression. At each point, the
/// regex will attempt to match the current state against the current input.
/// </summary>
public static class State
{
    public static string QuantifierToString(Quantifier quantifier, int maximum)
    {
        return quantifier switch
        {
            Quantifier.Range => $"0-{maximum}",
            _ => quantifier.ToString()
        };
    }

    public static IState EndOfInput { get; } = new EndAnchorState("end of input");

    public static IState EndSentinel { get; } = new EndSentinelState("end sentinel");

    public static IState Fence { get; } = new FenceState("fence")
    {
        Quantifier = Quantifier.ZeroOrOne
    };

    public static IState CreateMatchState(char c)
        => CreateMatchState(x => x == c, $"Matches {c}");

    private static IState VerifyLastStateIsNotEndAnchor(List<IState> states)
    {
        var previousState = states.LastOrDefault();
        if (previousState is EndAnchorState)
            throw new RegexException("Cannot add more states, or quantifiers, to end anchor$");
        return previousState;
    }

    public static List<IState> AddMatch(List<IState>? states, Func<char, bool> predicate, string description)
    {
        states ??= new List<IState>();
        VerifyLastStateIsNotEndAnchor(states);

        states.Add(CreateMatchState(predicate, description));
        return states;
    }

    private static IState CreateMatchState(Func<char, bool> predicate, string description)
         => new MatchValueState(description, predicate)
         {
             Quantifier = Quantifier.ExactlyOne,
         };

    public static List<IState> SetPreviousStateRange(List<IState> states, int min, int max)
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
        if (quantifier == Quantifier.Range)
            throw new RegexException("Range quantifier must have minimum and maximum values");

        var previousState = VerifyLastStateIsNotEndAnchor(states);

        if (previousState.Quantifier != Quantifier.ExactlyOne)
            throw new RegexException("Quantifier may only follow an unquantified atom");

        previousState.Quantifier = quantifier;
        return states;
    }

    public static List<IState> AddSpecialMatch(List<IState>? states, char type)
    {
        states ??= new List<IState>();
        VerifyLastStateIsNotEndAnchor(states);

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

    private static IState CreateBackreferenceState(int index)
    {
        return new MatchBackreferenceState($"Match backreference {index}", index)
        {
            GroupNumber = index,
        };
    }

    public static List<IState> AddCapturingGroupState(List<IState>? states, List<IState> group)
    {
        states ??= new List<IState>();
        VerifyLastStateIsNotEndAnchor(states);

        int groupNumber = states.Count + 1;
        var groupState = new CapturingGroupState($"group {groupNumber}", groupNumber, group)
        {
            Quantifier = Quantifier.ExactlyOne
        };
        states.Add(groupState);
        return states;
    }

    public static List<IState> AddNonCapturingCloisterState(List<IState>? states, List<IState> group)
    {
        states ??= new List<IState>();
        VerifyLastStateIsNotEndAnchor(states);

        // Minor optimization. Since we're not capturing, a cloister of 1 is the same as the inner
        // state with no cloister
        if (group.Count == 1)
        {
            states.Add(group[0]);
            return states;
        }

        int groupNumber = states.Count + 1;
        var groupState = new NonCapturingCloisterState($"cloister {groupNumber}", group)
        {
            Quantifier = Quantifier.ExactlyOne
        };
        states.Add(groupState);
        return states;
    }

    public static List<IState> QuantifyPrevious(List<IState> states, Quantifier quantifier)
    {
        return SetPreviousQuantifier(states, quantifier);
    }
}
