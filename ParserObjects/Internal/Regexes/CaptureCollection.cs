using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes;

public sealed class CaptureCollection : List<(int group, string value)>
{
    /* CaptureCollection IS-A List to avoid a second allocation. We cannot make it a struct because
     * we need reference behavior on the CaptureIndex value.
     *
     * CaptureIndex points to the most recently added item. The Regex Engine will include that
     * value in snapshots so that it can rewind to a previous index during backtracking.
     */

    /* Notice: We allocate the CaptureCollection. Then on successful match we allocate the array
     * of captures. Then in the RegexParser we allocate an array of objects to hold data, allocate
     * a new RegexMatch object, which in the constructor allocates two dictionaries. I recognize
     * this as a problem, but cannot think of any obvious, meaningful solution which doesn't involve
     * all sorts of ugly unsafe pointer nonsense.
     */

    public CaptureCollection()
    {
        CaptureIndex = -1;
    }

    public int CaptureIndex { get; private set; }

    public int AddCapture(int group, string value)
    {
        int currentIndex = CaptureIndex + 1;
        if (Count > currentIndex)
        {
            CaptureIndex++;
            this[CaptureIndex] = (group, value);
            return CaptureIndex;
        }

        Add((group, value));
        CaptureIndex = Count - 1;

        return CaptureIndex;
    }

    public void ResetCaptureIndex(int captureIndex)
    {
        CaptureIndex = captureIndex >= Count ? Count - 1 : captureIndex;
    }

    public RegexMatch ToRegexMatch(string overallMatch)
    {
        if (CaptureIndex < 0)
        {
            return new RegexMatch
            {
                { 0, new[] { overallMatch } }
            };
        }

        var groups = new RegexMatch
        {
            { 0, new[] { overallMatch } }
        };

        for (int i = 0; i <= CaptureIndex; i++)
        {
            var (group, value) = this[i];
            Debug.Assert(group > 0, "We cannot add to group 0");
            if (!groups.ContainsKey(group))
                groups.Add(group, new List<string>());
            (groups[group] as List<string>)!.Add(value);
        }

        return groups;
    }

    public string? GetLatestValueForGroup(int groupNumber)
    {
        for (int i = CaptureIndex; i >= 0; i--)
        {
            if (this[i].group == groupNumber)
                return this[i].value;
        }

        return null;
    }
}
