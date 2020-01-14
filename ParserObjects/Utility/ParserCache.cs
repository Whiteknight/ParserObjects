using System;
using System.Collections.Generic;

namespace ParserObjects.Utility
{
    /// <summary>
    /// Cache for parser objects
    /// </summary>
    public class ParserCache
    {
        private static ParserCache _instance;

        private readonly Dictionary<string, object> _cache;

        public ParserCache()
        {
            _cache = new Dictionary<string, object>();
        }

        public static ParserCache Instance => _instance ??= new ParserCache();

        public T GetParser<T>(string name, Func<T> getParser)
            where T : class, IParser
        {
            if (!_cache.ContainsKey(name))
                _cache.Add(name, getParser());

            return _cache[name] as T;
        }

    }
}
