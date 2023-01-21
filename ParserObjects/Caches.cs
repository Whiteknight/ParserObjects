using Microsoft.Extensions.Caching.Memory;
using ParserObjects.Internal.Caching;

namespace ParserObjects;

/// <summary>
/// Factory methods for creating caches.
/// </summary>
public static class Caches
{
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
    /// Create a null cache which does not cache any values.
    /// </summary>
    /// <returns></returns>
    public static IResultsCache NullCache()
        => new NullResultsCache();
}
