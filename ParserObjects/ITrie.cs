using System.Collections.Generic;
using ParserObjects.Parsers;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// A trie type which allows using a composite key to search for values
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ITrie<TKey, TResult>
    {
        // TODO: Should we try to separate the read/write bits into separate interfaces, since we don't need both at the same time?

        /// <summary>
        /// Given a composite key and a value, insert the value at the location described by the key
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        ITrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result);

        /// <summary>
        /// Given a composite key, search for a value at that location in the trie
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        IParseResult<TResult> Get(IEnumerable<TKey> keys);

        /// <summary>
        /// Given a sequence, treat the items in that sequence as elements of a composite key. Return a
        /// value from the trie which successfully consumes the most amount of input items.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        IParseResult<TResult> Get(ISequence<TKey> keys);
    }

    public static class TrieExtensions
    {
        public static IParser<TKey, TResult> ToParser<TKey, TResult>(this ITrie<TKey, TResult> trie)
            => new TrieParser<TKey, TResult>(trie);

        public static ITrie<char, string> Add(this ITrie<char, string> trie, string value)
        {
            Assert.ArgumentNotNull(trie, nameof(trie));
            return trie.Add(value, value);
        }

        public static ITrie<char, string> AddMany(this ITrie<char, string> trie, params string[] values)
        {
            Assert.ArgumentNotNull(trie, nameof(trie));
            Assert.ArgumentNotNull(values, nameof(values));
            foreach (var value in values)
                trie.Add(value, value);
            return trie;
        }
    }
}