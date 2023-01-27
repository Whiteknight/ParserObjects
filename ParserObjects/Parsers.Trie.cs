using System;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Tries;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Look up sequences of inputs in an IReadOnlyTrie to greedily find the longest matching
    /// sequence.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="trie"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Trie<TOutput>(InsertableTrie<TInput, TOutput> trie)
        where TOutput : notnull
    {
        var readable = ReadableTrie<TInput, TOutput>.Create(trie);
        return new TrieParser<TInput, TOutput>(readable);
    }

    /// <summary>
    /// Lookup sequences of inputs in an trie to greedily find the longest matching sequence.
    /// Provides a trie instance and a callback to populate it with values.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setupTrie"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Trie<TOutput>(Action<InsertableTrie<TInput, TOutput>> setupTrie)
        => TrieParser<TInput, TOutput>.Configure(setupTrie);

    /// <summary>
    /// Lookup a sequences of inputs in an IReadOnlyTrie and return all matches from the current
    /// position.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="trie"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> TrieMulti<TOutput>(InsertableTrie<TInput, TOutput> trie)
        where TOutput : notnull
    {
        var readable = ReadableTrie<TInput, TOutput>.Create(trie);
        return new TrieParser<TInput, TOutput>(readable);
    }

    /// <summary>
    /// Lookup sequences of inputs in a trie and return all matches from the current position.
    /// Provides a trie instance and a callback to populate it with values.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setupTrie"></param>
    /// <returns></returns>
    public static IMultiParser<TInput, TOutput> TrieMulti<TOutput>(Action<InsertableTrie<TInput, TOutput>> setupTrie)
        => TrieParser<TInput, TOutput>.ConfigureMulti(setupTrie);
}
