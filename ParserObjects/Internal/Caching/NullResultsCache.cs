namespace ParserObjects.Internal.Caching;

public sealed class NullResultsCache : IResultsCache
{
    private CacheStatistics _stats;

    public void Add<TValue>(ISymbol symbol, Location location, TValue value)
    {
        // No cache, so we do nothing here
    }

    public Option<TValue> Get<TValue>(ISymbol symbol, Location location)
    {
        _stats.Attempts++;
        return default;
    }

    public ICacheStatistics GetStatistics() => _stats;
}
