using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Internal.Visitors;

/// <summary>
/// Contains the results of multiple replaces from the parser graph.
/// </summary>
public readonly struct MultiReplaceResult
{
    public IReadOnlyList<SingleReplaceResult> Results { get; }

    /// <summary>
    /// Gets a value indicating whether all replacements succeeded. False if no attempts were
    /// made or if any result
    /// failed.
    /// </summary>
    public bool Success { get; }

    public MultiReplaceResult(IReadOnlyList<SingleReplaceResult> results)
    {
        Results = results;
        Success = results?.Count > 0 && results.All(r => r.Success);
    }

    public static MultiReplaceResult Failure() => default;

    public readonly void Deconstruct(out bool success, out IReadOnlyList<SingleReplaceResult> results)
    {
        success = Success;
        results = Results;
    }
}
