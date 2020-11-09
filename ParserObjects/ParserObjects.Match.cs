using System;
using System.Collections.Generic;
using ParserObjects.Parsers;
using ParserObjects.Utility;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        /// <summary>
        /// Test the next input value and return it, if it matches the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IParser<TInput, TInput> Match(Func<TInput, bool> predicate)
            => new MatchPredicateParser<TInput>(predicate);

        /// <summary>
        /// Get the next input value and return it if it .Equals() to the given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IParser<TInput, TInput> Match(TInput pattern)
            => Match(s => s.Equals(pattern));

        /// <summary>
        /// Get the next few input values and compare them one-by-one against an ordered sequence of test
        /// values. If every value in the sequence matches, return the sequence as a list.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TInput>> Match(IEnumerable<TInput> pattern)
            => new MatchSequenceParser<TInput>(pattern);

        /// <summary>
        /// Optimized implementation of First() which returns an input which matches any of the given pattern
        /// strings. Uses a Trie internally to greedily match the longest matching input sequence
        /// </summary>
        /// <param name="patterns"></param>
        /// <returns></returns>
        public static IParser<char, string> MatchAny(IEnumerable<string> patterns)
        {
            var trie = new InsertOnlyTrie<char, string>();
            foreach (var pattern in patterns)
                trie.Add(pattern, pattern);
            return new TrieParser<char, string>(trie);
        }
    }
}
