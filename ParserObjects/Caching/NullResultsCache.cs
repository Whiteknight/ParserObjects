namespace ParserObjects.Caching
{
    public class NullResultsCache : IResultsCache
    {
        private int _attempts;

        public NullResultsCache()
        {
            _attempts = 0;
        }

        public void Add<TValue>(ISymbol parser, Location location, TValue value)
        {
            // No cache, so we do nothing here
        }

        public IOption<TValue> Get<TValue>(ISymbol parser, Location location)
        {
            _attempts++;
            return FailureOption<TValue>.Instance;
        }

        public ICacheStatistics GetStatistics() => new CacheStatistics(_attempts, 0, _attempts);
    }
}
