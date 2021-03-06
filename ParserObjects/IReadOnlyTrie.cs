﻿using System.Collections.Generic;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects
{
    /// <summary>
    /// A trie type which allows using a composite key to search for values. This trie cannot be
    /// modified once created.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IReadOnlyTrie<TKey, out TResult>
    {
        /// <summary>
        /// Given a sequence, treat the items in that sequence as elements of a composite key. Return a
        /// value from the trie which successfully consumes the most amount of input items.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        IPartialResult<TResult> Get(ISequence<TKey> keys);

        /// <summary>
        /// Get all the pattern sequences in the trie. This operation may iterate over the entire trie so
        /// the results should be cached if possible.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IReadOnlyList<TKey>> GetAllPatterns();
    }

    /// <summary>
    /// A trie to which items can be added.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IInsertableTrie<TKey, TResult> : IReadOnlyTrie<TKey, TResult>
    {
        /// <summary>
        /// Given a composite key and a value, insert the value at the location described by the key.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        IInsertableTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result);
    }

    public static class TrieExtensions
    {
        /// <summary>
        /// Convenience method to add a string value with char keys.
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
        /// Convenience method to add strings to the trie with char keys.
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

        /// <summary>
        /// Get a value from a char trie. Used mostly for testing purposes.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="trie"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static IOption<TResult> Get<TResult>(this IReadOnlyTrie<char, TResult> trie, string keys)
        {
            var input = new StringCharacterSequence(keys);
            var result = trie.Get(input);
            return result.Success ? new SuccessOption<TResult>(result.Value) : FailureOption<TResult>.Instance;
        }
    }
}
