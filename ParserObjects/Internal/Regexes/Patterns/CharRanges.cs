using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Regexes.Patterns;

public readonly record struct CharRanges(byte[]? ExactChars, List<(char Low, char High)>? Ranges)
{
    public bool HasAny => ExactChars != null || Ranges != null;

    public CharRanges Add(char low, char high)
    {
        if (high < low)
            throw new RegexException($"Invalid range {high}-{low} should be {low}-{high}");

        if (low >= ' ' && high <= '~')
        {
            var chars = ExactChars ?? new byte[12];
            Debug.Assert(chars.Length == 12);
            for (char c = low; c <= high; c++)
                SetCharBit(chars, c);
            return this with { ExactChars = chars };
        }

        var ranges = Ranges ?? [];
        ranges.Add((low, high));
        return this with { Ranges = ranges };
    }

    private static void SetCharBit(byte[] chars, char c)
    {
        var intVal = c - ' ';
        chars[intVal / 8] |= (byte)(1 << intVal % 8);
    }
}
