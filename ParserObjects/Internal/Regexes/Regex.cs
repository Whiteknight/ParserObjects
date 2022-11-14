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
        NumberGroups(states);
    }

    private void NumberGroups(IReadOnlyList<State> states)
    {
        NumberGroups(states, 0);
    }

    private int NumberGroups(IReadOnlyList<State> states, int num)
    {
        foreach (var state in states)
        {
            if (state.Type == StateType.Group)
            {
                state.GroupNumber = num++;
                num = NumberGroups(state.Group!, num);
            }
        }

        return num;
    }
}
