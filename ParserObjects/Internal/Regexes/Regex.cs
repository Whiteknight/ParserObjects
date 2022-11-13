using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Regexes;

/// <summary>
/// Represents a compiled regex pattern. Used by the RegexEngine to perform a match.
/// </summary>
public struct Regex
{
    public IReadOnlyList<State> States { get; }

    public Regex(IReadOnlyList<State> states)
    {
        Assert.ArgumentNotNull(states, nameof(states));
        States = states;
        Debug.Assert(States.All(s => s != null), "There are null states in the regex list");
    }
}
