using System;
using System.Collections.Generic;

namespace ParserObjects.Regexes
{
    /// <summary>
    /// Represents a state at a point in evaluating a regular expression. At each point, the
    /// regex will attempt to match the current state against the current input
    /// </summary>
    public class RegexState
    {
        /// <summary>
        /// The type of state
        /// </summary>
        public RegexStateType Type { get; set; }

        /// <summary>
        /// Whether or not this state supports backtracking.
        /// </summary>
        public bool CanBacktrack { get; set; }

        /// <summary>
        /// The quantifier that controls how many times this state should match.
        /// </summary>
        public Quantifier Quantifier { get; set; }

        /// <summary>
        /// A predicate that determines whether a value matches
        /// </summary>
        public Func<char, bool> ValuePredicate { get; set; }

        /// <summary>
        /// All substates if this state is a group
        /// </summary>
        public List<RegexState> Group { get; set; }

        /// <summary>
        /// All possibilities in an alternation
        /// </summary>
        public List<List<RegexState>> Alternations { get; set; }

        /// <summary>
        /// A brief description of the state, to help with tracing/debugging
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The maximum number of times this state can match, if it supports more than one. Used
        /// with Range quantifiers
        /// </summary>
        public int Maximum { get; set; }

        public override string ToString() => $"{Quantifier} {Description}";
    }
}
