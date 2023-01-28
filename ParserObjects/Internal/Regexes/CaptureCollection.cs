using System;
using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public sealed class CaptureCollection : List<(int group, string value)>
{
    /* CaptureCollection IS-A List to avoid a second allocation. We cannot make it a struct because
     * we need reference behavior on the CaptureIndex value.
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

    public IReadOnlyList<(int group, string value)> ToList()
    {
        // CaptureCollection IS-A list, but we cannot use it as the list itself because it may
        // contain more items than required. So we have to slice the list and only return up to
        // CaptureIndex items.
        if (CaptureIndex < 0)
            return Array.Empty<(int, string)>();

        return GetRange(0, CaptureIndex + 1);
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
