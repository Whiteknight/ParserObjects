using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Tries;
using ParserObjects.Internal.Utility;
using static ParserObjects.Sequences;

namespace ParserObjects;

/// <summary>
/// Trie implementation which allows inserts of values but not updates of values. Once a value is
/// inserted into the trie, it cannot be removed or modified.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TResult"></typeparam>
public struct InsertableTrie<TKey, TResult>
{
    private readonly Node<TKey, TResult> _root;
    private readonly List<IReadOnlyList<TKey>> _patterns;

    private InsertableTrie(Node<TKey, TResult> root, List<IReadOnlyList<TKey>> patterns)
    {
        _root = root;
        _patterns = patterns;
    }

    public static InsertableTrie<TKey, TResult> Create()
        => new InsertableTrie<TKey, TResult>(new Node<TKey, TResult>(), new List<IReadOnlyList<TKey>>());

    public static InsertableTrie<TKey, TResult> Setup(Action<InsertableTrie<TKey, TResult>> setup)
    {
        var trie = Create();
        setup?.Invoke(trie);
        return trie;
    }

    public int Count => _patterns.Count;

    public ReadableTrie<TKey, TResult> Freeze() => new ReadableTrie<TKey, TResult>(_root, _patterns);

    public InsertableTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result)
    {
        Assert.ArgumentNotNull(keys, nameof(keys));
        Assert.ArgumentNotNull(result, nameof(result));
        var current = _root;
        var keyList = keys.ToArray();
        foreach (var key in keyList)
            current = current.GetOrAdd(key);

        if (current.TryAddResult(result))
            _patterns.Add(keyList);
        return this;
    }
}

public struct ReadableTrie<TKey, TResult>
{
    private readonly Node<TKey, TResult> _root;
    private readonly IReadOnlyList<IReadOnlyList<TKey>> _patterns;

    public ReadableTrie(Node<TKey, TResult> root, IReadOnlyList<IReadOnlyList<TKey>> patterns)
    {
        _root = root;
        _patterns = patterns;
    }

    public PartialResult<TResult> Get(ISequence<TKey> keys)
    {
        Assert.ArgumentNotNull(keys, nameof(keys));
        return Node<TKey, TResult>.Get(_root, keys);
    }

    public bool CanGet(ISequence<TKey> keys)
    {
        Assert.ArgumentNotNull(keys, nameof(keys));
        return Node<TKey, TResult>.CanGet(_root, keys);
    }

    public IReadOnlyList<IResultAlternative<TResult>> GetMany(ISequence<TKey> keys)
    {
        Assert.ArgumentNotNull(keys, nameof(keys));
        return Node<TKey, TResult>.GetMany(_root, keys);
    }

    public IEnumerable<IReadOnlyList<TKey>> GetAllPatterns() => _patterns;
}

public static class TrieExtensions
{
    /// <summary>
    /// Add a string value to a char trie as it's own key.
    /// </summary>
    /// <param name="readOnlyTrie"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static InsertableTrie<char, string> Add(
        this InsertableTrie<char, string> readOnlyTrie,
        string value
    )
    {
        return readOnlyTrie.Add(value, value);
    }

    /// <summary>
    /// Get a string value from a char trie.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="trie"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static Option<TResult> Get<TResult>(this ReadableTrie<char, TResult> trie, string keys)
    {
        var input = FromString(keys, new SequenceOptions<char>
        {
            MaintainLineEndings = true,
            Encoding = System.Text.Encoding.ASCII
        });
        var result = trie.Get(input);
        return result.Match(default, value => new Option<TResult>(true, value));
    }
}
