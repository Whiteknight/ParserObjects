using System;
using System.Collections.Generic;

namespace ParserObjects.Regexes
{
    /// <summary>
    /// Represents a state at a point in evaluating a regular expression. At each point, the
    /// regex will attempt to match the current state against the current input.
    /// </summary>
    public class RegexState
    {
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
        public Func<char, bool> ValuePredicate { get; set; }

        /// <summary>
        /// Gets or sets all substates if this state is a group.
        /// </summary>
        public List<RegexState> Group { get; set; }

        /// <summary>
        /// Gets or sets all possibilities in an alternation.
        /// </summary>
        public List<List<RegexState>> Alternations { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the state, to help with tracing/debugging.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times this state can match, if it supports more than
        /// one. Used only with Range quantifiers.
        /// </summary>
        public int Maximum { get; set; }

        public override string ToString() => $"{Quantifier} {Description}";
    }
}
