using System.Collections.Generic;
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
        /// <summary>
        /// Given a composite key and a value, insert the value at the location described by the key
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        InsertOnlyTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result);

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
}