using System;
using ParserObjects.Parsers;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Look up sequences of inputs in an ITrie to greedily find the longest matching sequence.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="readOnlyTrie"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Trie<TOutput>(IReadOnlyTrie<TInput, TOutput> readOnlyTrie)
            => new TrieParser<TInput, TOutput>(readOnlyTrie);

        /// <summary>
        /// Lookup sequences of inputs in an ITrie to greedily find the longest matching sequence.
        /// Provides a trie instance and a callback to populate it with values.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="setupTrie"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Trie<TOutput>(Action<IInsertableTrie<TInput, TOutput>> setupTrie)
        {
            var trie = new InsertOnlyTrie<TInput, TOutput>();
            setupTrie?.Invoke(trie);
            return new TrieParser<TInput, TOutput>(trie);
        }
    }
}
