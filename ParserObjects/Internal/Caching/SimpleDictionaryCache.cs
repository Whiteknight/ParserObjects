using System.Collections.Generic;

namespace ParserObjects.Internal.Caching;

public sealed class SimpleDictionaryCache : Dictionary<(ISymbol, Location), object?>, IResultsCache
{
    private CacheStatistics _statistics;

    public void Add<TValue>(ISymbol symbol, Location location, TValue value)
    {
        this[(symbol, location)] = value;
    }

    public Option<TValue> Get<TValue>(ISymbol symbol, Location location)
    {
        _statistics.Attempts++;
        if (TryGetValue((symbol, location), out var value) && value is TValue typed)
        {
            _statistics.Hits++;
            return new Option<TValue>(true, typed);
        }

        _statistics.Misses++;
        return default;
    }

    public ICacheStatistics GetStatistics() => _statistics;
}
