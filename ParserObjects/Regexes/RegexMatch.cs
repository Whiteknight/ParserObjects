using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Regexes;

/// <summary>
/// A match object which includes the overall matched string and the contents of all capturing
/// groups.
/// </summary>
public class RegexMatch
{
    public RegexMatch(string overallMatch, IReadOnlyList<(int group, string value)> captures)
    {
        var groups = new Dictionary<int, List<string>>()
        {
            { 0, new List<string> { overallMatch } }
        };

        foreach (var (group, value) in captures)
        {
            if (!groups.ContainsKey(group))
                groups.Add(group, new List<string>());
            groups[group].Add(value);
        }

        Groups = groups.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<string>)kvp.Value);
    }

    /// <summary>
    /// GEts the captured groups. Group 0 is the overall match of the regex. Other groups are numbered
    /// starting at 1, based on the order of opening parenthesis in the regex.
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyList<string>> Groups { get; }
}
