using System.Collections.Generic;

namespace ParserObjects.Internal.Utility;

/// <summary>
/// Result of a single replacement operation
/// </summary>
public record struct SingleReplaceResult(
    IReplaceableParserUntyped Replaceable,
    IParser Previous,
    IParser Current
)
{
    /// <summary>
    /// Gets a value indicating whether the replace happened, false if it did not.
    /// </summary>
    public bool Success => !ReferenceEquals(Previous, Current);

    /// <summary>
    /// Get the previous and current parser value.
    /// </summary>
    /// <param name="success"></param>
    /// <param name="previous"></param>
    /// <param name="current"></param>
    public void Deconstruct(out bool success, out IParser previous, out IParser current)
    {
        success = Success;
        previous = Previous;
        current = Current;
    }

    /// <summary>
    /// Get the previous parser, current parser, and the ReplaceableParser parent.
    /// </summary>
    /// <param name="success"></param>
    /// <param name="previous"></param>
    /// <param name="current"></param>
    /// <param name="replaceable"></param>
    public void Deconstruct(out bool success, out IParser previous, out IParser current, out IReplaceableParserUntyped replaceable)
    {
        success = Success;
        previous = Previous;
        current = Current;
        replaceable = Replaceable;
    }
}

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

    public void Deconstruct(out bool success, out IReadOnlyList<SingleReplaceResult> results)
    {
        success = Success;
        results = Results;
    }
}
