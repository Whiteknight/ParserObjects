using System;
using Microsoft.Extensions.Caching.Memory;

namespace ParserObjects.Caching
{
    public sealed class MemoryCacheResultsCache : IResultsCache, IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly bool _ownsCache;

        private int _attempts;
        private int _hits;
        private int _misses;

        public MemoryCacheResultsCache()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _ownsCache = true;
            _attempts = 0;
            _hits = 0;
            _misses = 0;
        }

        public MemoryCacheResultsCache(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _ownsCache = false;
            _attempts = 0;
            _hits = 0;
            _misses = 0;
        }

        private struct Key
        {
            public Key(ISymbol symbol, Location location)
            {
                Symbol = symbol;
                Location = location;
            }

            public ISymbol Symbol { get; }
            public Location Location { get; }
        }

        public void Add<TValue>(ISymbol parser, Location location, TValue value)
        {
            var key = new Key(parser, location);
            _cache.Set(key, value);
        }

        public void Dispose()
        {
            if (_ownsCache)
                _cache.Dispose();
        }

        public IOption<TValue> Get<TValue>(ISymbol parser, Location location)
        {
            _attempts++;
            var key = new Key(parser, location);
            if (_cache.TryGetValue(key, out var objValue) && objValue is TValue typed)
            {
                _hits++;
                return new SuccessOption<TValue>(typed);
            }

            _misses++;
            return FailureOption<TValue>.Instance;
        }

        public ICacheStatistics GetStatistics() => new CacheStatistics(_attempts, _hits, _misses);
    }
}
