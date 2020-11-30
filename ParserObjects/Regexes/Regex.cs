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

        public Regex(IRegexNode regex)
        {
            Assert.ArgumentNotNull(regex, nameof(regex));
            var states = new List<List<RegexState>> { new List<RegexState>() };
            regex.BuildUpStates(states);
            if (states.Count != 1)
                throw new RegexException("Invalid regular expression. Too many incomplete groups");
            States = states[0];
            Debug.Assert(States.All(s => s != null));
        }
    }
}
