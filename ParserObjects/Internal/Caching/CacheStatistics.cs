namespace ParserObjects.Internal.Caching;

public struct CacheStatistics : ICacheStatistics
{
    public int Attempts { get; set; }
    public int Hits { get; set; }
    public int Misses { get; set; }
}
