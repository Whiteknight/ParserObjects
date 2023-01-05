using System;
using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public sealed class CaptureCollection
{
    private readonly List<(int group, string value)> _captures;

    public CaptureCollection()
    {
        _captures = new List<(int, string)>();
        CaptureIndex = -1;
    }

    public int CaptureIndex { get; private set; }

    public int AddCapture(int group, string value)
    {
        int currentIndex = CaptureIndex + 1;
        if (_captures.Count > currentIndex)
        {
            CaptureIndex++;
            _captures[CaptureIndex] = (group, value);
            return CaptureIndex;
        }

        _captures.Add((group, value));
        CaptureIndex = _captures.Count - 1;

        return CaptureIndex;
    }

    public void ResetCaptureIndex(int captureIndex)
    {
        CaptureIndex = captureIndex >= _captures.Count ? _captures.Count - 1 : captureIndex;
    }

    public IReadOnlyList<(int group, string value)> ToList()
    {
        if (CaptureIndex < 0)
            return Array.Empty<(int, string)>();
        var result = new (int, string)[CaptureIndex + 1];
        for (int i = 0; i <= CaptureIndex; i++)
            result[i] = _captures[i];
        return result;
    }

    public string? GetLatestValueForGroup(int groupNumber)
    {
        for (int i = CaptureIndex; i >= 0; i--)
        {
            if (_captures[i].group == groupNumber)
                return _captures[i].value;
        }

        return null;
    }
}
