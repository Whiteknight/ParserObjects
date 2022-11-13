using Microsoft.Extensions.Caching.Memory;
using ParserObjects.Internal.Caching;

namespace ParserObjects;

public static class Caches
{
    public static IResultsCache InMemoryCache()
        => new MemoryCacheResultsCache();

    public static IResultsCache InMemoryCache(IMemoryCache memoryCache)
        => new MemoryCacheResultsCache(memoryCache);

    public static IResultsCache NullCache()
        => new NullResultsCache();
}
