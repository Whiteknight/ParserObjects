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
    public interface IReadOnlyTrie<TKey, TResult>
    {
        /// <summary>
        /// Given a composite key, search for a value at that location in the trie
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        (bool Success, TResult Value) Get(IEnumerable<TKey> keys);

        /// <summary>
        /// Given a sequence, treat the items in that sequence as elements of a composite key. Return a
        /// value from the trie which successfully consumes the most amount of input items.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        (bool Success, TResult Value, Location location) Get(ISequence<TKey> keys);

        /// <summary>
        /// Get all the pattern sequences in the trie. This operation may iterate over the entire trie so
        /// the results should be cached if possible.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IReadOnlyList<TKey>> GetAllPatterns();
    }

    public interface IInsertableTrie<TKey, TResult> : IReadOnlyTrie<TKey, TResult>
    {
        /// <summary>
        /// Given a composite key and a value, insert the value at the location described by the key
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        IInsertableTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result);
    }

    public static class TrieExtensions
    {
        /// <summary>
        /// Wrap the Trie in a TrieParser
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="readOnlyTrie"></param>
        /// <returns></returns>
        public static IParser<TKey, TResult> ToParser<TKey, TResult>(this IReadOnlyTrie<TKey, TResult> readOnlyTrie)
            => new TrieParser<TKey, TResult>(readOnlyTrie);

        /// <summary>
        /// Convenience method to add a string value with char keys
        /// </summary>
        /// <param name="readOnlyTrie"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IInsertableTrie<char, string> Add(this IInsertableTrie<char, string> readOnlyTrie, string value)
        {
            Assert.ArgumentNotNull(readOnlyTrie, nameof(readOnlyTrie));
            return readOnlyTrie.Add(value, value);
        }

        /// <summary>
        /// Convenience method to add strings to the trie with char keys
        /// </summary>
        /// <param name="readOnlyTrie"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IInsertableTrie<char, string> AddMany(this IInsertableTrie<char, string> readOnlyTrie, params string[] values)
        {
            Assert.ArgumentNotNull(readOnlyTrie, nameof(readOnlyTrie));
            Assert.ArgumentNotNull(values, nameof(values));
            foreach (var value in values)
                readOnlyTrie.Add(value, value);
            return readOnlyTrie;
        }
    }
}