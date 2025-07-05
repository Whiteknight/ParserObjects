using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal;
using ParserObjects.Internal.Tries;

namespace ParserObjects;

/// <summary>
/// Trie implementation which allows inserts of values but not updates of values. Once a value is
/// inserted into the trie, it cannot be removed or modified.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TResult"></typeparam>
public readonly struct InsertableTrie<TKey, TResult>
{
    private readonly RootNode<TKey, TResult> _root;
    private readonly List<IReadOnlyList<TKey>> _patterns;

    private InsertableTrie(RootNode<TKey, TResult> root, List<IReadOnlyList<TKey>> patterns)
    {
        _root = root;
        _patterns = patterns;
    }

    /// <summary>
    /// Create a new instance using the given equality comparer.
    /// </summary>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static InsertableTrie<TKey, TResult> Create(IEqualityComparer<TKey>? comparer = null)
        => comparer == null
        ? new InsertableTrie<TKey, TResult>(new RootNode<TKey, TResult>(), [])
        : new InsertableTrie<TKey, TResult>(new RootNode<TKey, TResult>(comparer), []);

    /// <summary>
    /// Create and initialize a new trie instance from a user callback.
    /// </summary>
    /// <param name="setup"></param>
    /// <param name="equalityComparer"></param>
    /// <returns></returns>
    public static InsertableTrie<TKey, TResult> Setup(Action<InsertableTrie<TKey, TResult>> setup, IEqualityComparer<TKey>? equalityComparer)
    {
        var trie = Create(equalityComparer);
        setup?.Invoke(trie);
        return trie;
    }

    /// <summary>
    /// Gets the count of items added to the trie.
    /// </summary>
    public readonly int Count => _patterns.Count;

    /// <summary>
    /// Adds a new item to the Trie with the given key pattern.
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public InsertableTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result)
    {
        Assert.NotNull(keys);
        Assert.NotNull(result);
        var keyList = keys.ToArray();
        if (_root.TryAdd(keyList, result))
            _patterns.Add(keyList);

        return this;
    }

    public void Deconstruct(out RootNode<TKey, TResult> root, out List<IReadOnlyList<TKey>> patterns)
    {
        root = _root;
        patterns = _patterns;
    }
}

public class TrieInsertException(string message) : Exception(message)
{
}

public static class TrieExtensions
{
    /// <summary>
    /// Add a string value to a char trie as it's own key.
    /// </summary>
    /// <param name="trie"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static InsertableTrie<char, string> Add(
        this InsertableTrie<char, string> trie,
        string value
    ) => trie.Add(value, value);
}
