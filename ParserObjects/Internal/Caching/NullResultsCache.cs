﻿namespace ParserObjects.Internal.Caching;

public sealed class NullResultsCache : IResultsCache
{
    private CacheStatistics _stats;

    public void Add<TValue>(ISymbol symbol, Location location, TValue value)
    {
        // No cache, so we do nothing here
    }

    public IOption<TValue> Get<TValue>(ISymbol symbol, Location location)
    {
        _stats.Attempts++;
        return FailureOption<TValue>.Instance;
    }

    public ICacheStatistics GetStatistics() => _stats;
}