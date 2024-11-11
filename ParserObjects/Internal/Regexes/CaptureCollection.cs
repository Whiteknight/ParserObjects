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

    private static readonly CaptureCollection _reusableInstance = new CaptureCollection();

    public CaptureCollection()
    {
        CaptureIndex = -1;
    }

    public int CaptureIndex { get; private set; }

    // If we know that a Regex has no capturing groups, we can just reuse an empty CaptureCollection
    // and not allocate a new one.
    public static CaptureCollection GetReusableInstance()
    {
        _reusableInstance.Clear();
        return _reusableInstance;
    }

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
        var groups = new RegexMatch
        {
            { 0, new[] { overallMatch } }
        };

        if (CaptureIndex < 0)
            return groups;

        for (int i = 0; i <= CaptureIndex; i++)
        {
            var (group, value) = this[i];
            Debug.Assert(group > 0, "We cannot add to group 0");
            if (!groups.ContainsKey(group))
                groups.Add(group, new List<string>());
            ((List<string>)groups[group]).Add(value);
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
