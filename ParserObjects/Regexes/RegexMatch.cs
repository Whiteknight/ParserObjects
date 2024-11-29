using System.Collections.Generic;

namespace ParserObjects.Regexes;

/// <summary>
/// A match object which includes the overall matched string and the contents of all capturing
/// groups. Group 0 is the overall match of the regex. Subsequence groupings are numbered based on
/// the ordering of the opening parenthesis for the grouping in the regex.
/// </summary>
public class RegexMatch : Dictionary<int, IReadOnlyList<string>>
{
    public static RegexMatch Empty { get; } = new RegexMatch
    {
        { 0, new[] { string.Empty } }
    };

    public IReadOnlyList<string> GetGroup(int group)
        => ContainsKey(group)
            ? this[group]
            : [];

    public string GetCapture(int group, int repetition = 0)
    {
        var captures = GetGroup(group);
        return captures.Count > repetition
            ? captures[repetition]
            : string.Empty;
    }
}
