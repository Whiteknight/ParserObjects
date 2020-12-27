namespace ParserObjects.Caching
{
    public struct CacheStatistics : ICacheStatistics
    {
        public CacheStatistics(int attempts, int hits, int misses)
        {
            Attempts = attempts;
            Hits = hits;
            Misses = misses;
        }

        public int Attempts { get; }
        public int Hits { get; }
        public int Misses { get; }
    }
}
