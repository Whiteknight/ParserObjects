using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Regexes
{
    /// <summary>
    /// Represents a state at a point in evaluating a regular expression. At each point, the
    /// regex will attempt to match the current state against the current input.
    /// </summary>
    public class RegexState : INamed
    {
        public RegexState(string name)
        {
            Name = name;
        }

        public static RegexState EndOfInput { get; } = new RegexState("end of input")
        {
            Type = RegexStateType.EndOfInput
        };

        public static RegexState EndSentinel { get; } = new RegexState("end sentinel")
        {
            Type = RegexStateType.EndSentinel
        };

        public static RegexState Fence { get; } = new RegexState("fence")
        {
            Type = RegexStateType.Fence,
            Quantifier = Quantifier.ZeroOrOne
        };

        public static RegexState CreateWildcardMatchState()
            => CreateMatchState(x => x != '\0', "Match any");

        public static RegexState CreateMatchState(char c)
            => CreateMatchState(x => x == c, $"Matches {c}");

        public static List<RegexState> AddMatch(List<RegexState>? states, Func<char, bool> predicate, string description)
        {
            states ??= new List<RegexState>();
            if (states.LastOrDefault()?.Type == RegexStateType.EndOfInput)
                throw new RegexException("Cannot have atoms after the end anchor $");

            states.Add(CreateMatchState(predicate, description));
            return states;
        }

        private static RegexState CreateMatchState(Func<char, bool> predicate, string description)
             => new RegexState(description)
             {
                 Type = RegexStateType.MatchValue,
                 ValuePredicate = predicate,
                 Quantifier = Quantifier.ExactlyOne,
             };

        public static List<RegexState> SetPreviousStateRange(List<RegexState> states, int min, int max)
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
            if (previousState.Type == RegexStateType.EndOfInput || previousState.Type == RegexStateType.Fence)
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

        public static List<RegexState> SetPreviousQuantifier(List<RegexState> states, Quantifier quantifier)
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

        public static List<RegexState> AddSpecialMatch(List<RegexState>? states, char type)
        {
            states ??= new List<RegexState>();
            if (states.LastOrDefault()?.Type == RegexStateType.EndOfInput)
                throw new RegexException("Cannot have atoms after the end anchor $");
            var matchState = type switch
            {
                'd' => CreateMatchState(c => char.IsDigit(c), "digit"),
                'D' => CreateMatchState(c => !char.IsDigit(c), "not digit"),
                'w' => CreateMatchState(c => char.IsLetterOrDigit(c) || c == '_', "word"),
                'W' => CreateMatchState(c => char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c), "not word"),
                's' => CreateMatchState(c => char.IsWhiteSpace(c), "whitespace"),
                'S' => CreateMatchState(c => !char.IsWhiteSpace(c), "not whitespace"),
                _ => CreateMatchState(type)
            };
            states.Add(matchState);
            return states;
        }

        public static List<RegexState> AddGroupState(List<RegexState>? states, List<RegexState> group)
        {
            states ??= new List<RegexState>();
            if (states.LastOrDefault()?.Type == RegexStateType.EndOfInput)
                throw new RegexException("Cannot have atoms after the end anchor $");
            if (group.Count == 1)
            {
                states.Add(group[0]);
                return states;
            }

            var groupState = new RegexState("group")
            {
                Type = RegexStateType.Group,
                Group = group,
                Quantifier = Quantifier.ExactlyOne
            };
            states.Add(groupState);
            return states;
        }

        public static List<RegexState> QuantifyPrevious(List<RegexState> states, Quantifier quantifier)
        {
            if (states.LastOrDefault()?.Type == RegexStateType.EndOfInput)
                throw new RegexException("Cannot quantify the end anchor $");
            return SetPreviousQuantifier(states, quantifier);
        }

        /// <summary>
        /// Gets or Sets the type of state.
        /// </summary>
        public RegexStateType Type { get; set; }

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
        public List<RegexState>? Group { get; set; }

        /// <summary>
        /// Gets or sets all possibilities in an alternation.
        /// </summary>
        public List<List<RegexState>>? Alternations { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the state, to help with tracing/debugging.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times this state can match, if it supports more than
        /// one. Used only with Range quantifiers.
        /// </summary>
        public int Maximum { get; set; }

        public override string ToString() => $"{Quantifier} {Name}";

        public RegexState Clone()
        {
            return new RegexState(Name)
            {
                Type = Type,
                CanBacktrack = CanBacktrack,
                Quantifier = Quantifier,
                ValuePredicate = ValuePredicate,
                Group = Group,
                Alternations = Alternations,
                Maximum = Maximum
            };
        }
    }
}
