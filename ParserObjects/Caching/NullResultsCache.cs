namespace ParserObjects.Caching
{
    public class NullResultsCache : IResultsCache
    {
        private int _attempts;

        public NullResultsCache()
        {
            _attempts = 0;
        }

        public void Add<TValue>(ICacheable producer, TValue value)
        {
        }

        public IOption<TValue> Get<TValue>(ICacheable key)
        {
            _attempts++;
            return FailureOption<TValue>.Instance;
        }

        public ICacheStatistics GetStatistics() => new CacheStatistics(_attempts, 0, _attempts);
    }
}
