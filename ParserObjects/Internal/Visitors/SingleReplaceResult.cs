namespace ParserObjects.Internal.Visitors;

/// <summary>
/// Result of a single replacement operation.
/// </summary>
public readonly record struct SingleReplaceResult(
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
