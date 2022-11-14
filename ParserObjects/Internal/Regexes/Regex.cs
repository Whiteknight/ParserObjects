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
        // The pattern parser numbers all Group states according to the index in the states array,
        // with "duplicate" groups having the same GroupNumber.
        // So "(.)+" will turn into "(.)(.)*" where both groups would have the same GroupNumber,
        // which is the array index of the first group.
        // This method goes through the list and tries to renumber these starting from 1,
        // depth-first. We do this by keeping track of the last "source group number" and incrementing
        // the "destination group number" only when necessary.
        NumberGroups(states, 0);
    }

    private int NumberGroups(IReadOnlyList<State> states, int destGroupNumber)
    {
        int lastSrcGroupNumber = -1;
        foreach (var state in states)
        {
            if (state.Type != StateType.CapturingGroup)
                continue;

            // If this is a new source groupNumber, keep track and increment the destination number
            if (lastSrcGroupNumber == -1 || state.GroupNumber != lastSrcGroupNumber)
            {
                destGroupNumber++;
                lastSrcGroupNumber = state.GroupNumber;
            }

            state.GroupNumber = destGroupNumber;
            var newNum = NumberGroups(state.Group!, destGroupNumber);
            if (newNum != destGroupNumber)
                lastSrcGroupNumber = -1;
        }

        return destGroupNumber;
    }
}
