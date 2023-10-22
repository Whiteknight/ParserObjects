using System;
using Microsoft.Extensions.Caching.Memory;

namespace ParserObjects.Internal.Caching;

public sealed class MemoryCacheResultsCache : IResultsCache, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly bool _ownsCache;

    private CacheStatistics _stats;

    public MemoryCacheResultsCache()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _ownsCache = true;
        _stats = default;
    }

    public MemoryCacheResultsCache(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _ownsCache = false;
        _stats = default;
    }

    private record struct Key(ISymbol Symbol, Location Location);

    public void Add<TValue>(ISymbol symbol, Location location, TValue value)
    {
        Assert.ArgumentNotNull(symbol);
        var key = new Key(symbol, location);
        _cache.Set(key, value);
    }

    public void Dispose()
    {
        if (_ownsCache)
            _cache.Dispose();
    }

    public Option<TValue> Get<TValue>(ISymbol symbol, Location location)
    {
        Assert.ArgumentNotNull(symbol);
        _stats.Attempts++;
        var key = new Key(symbol, location);
        if (_cache.TryGetValue(key, out var objValue) && objValue is TValue typed)
        {
            _stats.Hits++;
            return new Option<TValue>(true, typed);
        }

        _stats.Misses++;
        return default;
    }

    public ICacheStatistics GetStatistics() => _stats;
}
