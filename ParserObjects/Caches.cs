using Microsoft.Extensions.Caching.Memory;
using ParserObjects.Internal.Caching;

namespace ParserObjects;

/// <summary>
/// Factory methods for creating caches.
/// </summary>
public static class Caches
{
    /// <summary>
    /// Uses a simple Dictionary implementation as a cache.
    /// </summary>
    /// <returns></returns>
    public static IResultsCache Dictionary()
        => new SimpleDictionaryCache();

    /// <summary>
    /// Create an in-memory cache using the IMemoryCache default instance.
    /// </summary>
    /// <returns></returns>
    public static IResultsCache InMemoryCache()
        => new MemoryCacheResultsCache();

    /// <summary>
    /// Create an in-memory cache wrapping an existing IMemoryCache instance.
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <returns></returns>
    public static IResultsCache InMemoryCache(IMemoryCache memoryCache)
        => new MemoryCacheResultsCache(memoryCache);

    /// <summary>
    /// Create a null cache which does not cache any values. WARNING: Caching is used to implement
    /// behavior in several parser types. Using a null cache may lead to instable parses or
    /// incorrect results.
    /// </summary>
    /// <returns></returns>
    public static IResultsCache NullCache()
        => NullResultsCache.Instance;
}
