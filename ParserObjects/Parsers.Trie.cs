using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Tries;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Look up sequences of inputs in a trie to greedily find the longest matching
    /// sequence.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="trie"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Trie<TOutput>(InsertableTrie<TInput, TOutput> trie)
        => trie.Count == 0
            ? Produce<TOutput>(static () => default!)
            : new TrieParser<TInput, TOutput>(ReadableTrie<TInput, TOutput>.Create(trie));

    /// <summary>
    /// Lookup sequences of inputs in an trie to greedily find the longest matching sequence.
    /// Provides a trie instance and a callback to populate it with values.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setupTrie"></param>
    /// <param name="keyComparer"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Trie<TOutput>(Action<InsertableTrie<TInput, TOutput>> setupTrie, IEqualityComparer<TInput>? keyComparer = null)
        => Trie(InsertableTrie<TInput, TOutput>.Setup(setupTrie, keyComparer));

    /// <summary>
    /// Lookup a sequences of inputs in a trie and return all matches from the current
    /// position.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="trie"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> TrieMulti<TOutput>(InsertableTrie<TInput, TOutput> trie)
        => trie.Count == 0
            ? ProduceMulti(static () => Enumerable.Empty<TOutput>())
            : new TrieParser<TInput, TOutput>(ReadableTrie<TInput, TOutput>.Create(trie));

    /// <summary>
    /// Lookup sequences of inputs in a trie and return all matches from the current position.
    /// Provides a trie instance and a callback to populate it with values.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setupTrie"></param>
    /// <param name="keyComparer"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> TrieMulti<TOutput>(Action<InsertableTrie<TInput, TOutput>> setupTrie, IEqualityComparer<TInput>? keyComparer = null)
        => TrieMulti(InsertableTrie<TInput, TOutput>.Setup(setupTrie, keyComparer));
}
