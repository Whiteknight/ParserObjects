using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Regexes;

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

    public IReadOnlyDictionary<int, IReadOnlyList<string>> Groups { get; }
}
