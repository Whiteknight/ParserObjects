namespace ParserObjects
{
    /// <summary>
    /// A cache for holding parse results.
    /// </summary>
    public interface IResultsCache
    {
        /// <summary>
        /// Attempt to find a value in the cache. Returns success with the
        /// cached value if it exists, failure otherwise.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="parser"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        IOption<TValue> Get<TValue>(ISymbol parser, Location location);

        /// <summary>
        /// Adds a value to the cache with the given key.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="parser"></param>
        /// <param name="location"></param>
        /// <param name="value"></param>
        void Add<TValue>(ISymbol parser, Location location, TValue value);

        /// <summary>
        /// Get aggregated statistics of cache usage.
        /// </summary>
        /// <returns></returns>
        ICacheStatistics GetStatistics();
    }
}
