using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ParserObjects.Internal.Regexes;

/// <summary>
/// Represents a compiled regex pattern. Used by the RegexEngine to perform a match.
/// </summary>
public readonly struct Regex
{
    public IReadOnlyList<IState> States { get; }

    public Regex(IReadOnlyList<IState> states)
    {
        Assert.ArgumentNotNull(states);
        States = states;
        Debug.Assert(States.All(s => s != null), "There are null states in the regex list");
        NumberGroups(states);
    }

    private static void NumberGroups(IReadOnlyList<IState> states)
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

    private static int NumberGroups(IReadOnlyList<IState> states, int destGroupNumber)
    {
        int lastSrcGroupNumber = -1;
        for (int i = 0; i < states.Count; i++)
        {
            var state = states[i];
            if (state is not CapturingGroupState groupState)
                continue;

            // If this is a new source groupNumber, keep track and increment the destination number
            if (lastSrcGroupNumber == -1 || groupState.GroupNumber != lastSrcGroupNumber)
            {
                destGroupNumber++;
                lastSrcGroupNumber = groupState.GroupNumber;
            }

            groupState.GroupNumber = destGroupNumber;
            var newNum = NumberGroups(groupState.Group!, destGroupNumber);
            if (newNum != destGroupNumber)
                lastSrcGroupNumber = -1;
        }

        return destGroupNumber;
    }
}
