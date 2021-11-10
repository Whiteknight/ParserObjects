namespace ParserObjects.Caching
{
    public class NullResultsCache : IResultsCache
    {
        private int _attempts;

        public NullResultsCache()
        {
            _attempts = 0;
        }

        public void Add<TValue>(ICacheable key, TValue value)
        {
            // No cache, so we do nothing here
        }

        public IOption<TValue> Get<TValue>(ICacheable key)
        {
            _attempts++;
            return FailureOption<TValue>.Instance;
        }

        public ICacheStatistics GetStatistics() => new CacheStatistics(_attempts, 0, _attempts);
    }
}
