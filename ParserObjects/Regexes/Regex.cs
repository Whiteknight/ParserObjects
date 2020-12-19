using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Regexes
{
    /// <summary>
    /// Represents a compiled regex pattern. Used by the RegexEngine to perform a match.
    /// </summary>
    public class Regex
    {
        public IReadOnlyList<RegexState> States { get; }

        public Regex(List<RegexState> states)
        {
            Assert.ArgumentNotNull(states, nameof(states));
            States = states;
            Debug.Assert(States.All(s => s != null), "There are null states in the regex list");
        }
    }
}
