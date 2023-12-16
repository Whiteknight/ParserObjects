using System.Collections.Generic;

namespace ParserObjects.Internal.Visitors;

/// <summary>
/// Contains the results of multiple replaces from the parser graph.
/// </summary>
public struct MultiReplaceResult
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
        if (results == null || results.Count == 0)
        {
            Success = false;
            return;
        }

        for (int i = 0; i < results.Count; i++)
        {
            if (!results[i].Success)
            {
                Success = false;
                return;
            }
        }

        Success = true;
    }

    public static MultiReplaceResult Failure() => default;

    public readonly void Deconstruct(out bool success, out IReadOnlyList<SingleReplaceResult> results)
    {
        success = Success;
        results = Results;
    }
}
