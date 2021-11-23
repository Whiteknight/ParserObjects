namespace ParserObjects;

/// <summary>
/// Statistics for cache usage.
/// </summary>
public interface ICacheStatistics
{
    /// <summary>
    /// Gets how many attempts have been made to find a value in the cache.
    /// </summary>
    int Attempts { get; }

    /// <summary>
    /// Gets how many attempts have hit in the cache.
    /// </summary>
    int Hits { get; }

    /// <summary>
    /// Gets how many attempts have missed in the cache.
    /// </summary>
    int Misses { get; }
}
