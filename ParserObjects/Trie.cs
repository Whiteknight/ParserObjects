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

    /// <summary>
    /// Create a new instance.
    /// </summary>
    /// <returns></returns>
    public static InsertableTrie<TKey, TResult> Create()
        => new InsertableTrie<TKey, TResult>(new Node<TKey, TResult>(), new List<IReadOnlyList<TKey>>());

    /// <summary>
    /// Create and initialize a new trie instance from a user callback.
    /// </summary>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static InsertableTrie<TKey, TResult> Setup(Action<InsertableTrie<TKey, TResult>> setup)
    {
        var trie = Create();
        setup?.Invoke(trie);
        return trie;
    }

    /// <summary>
    /// Gets the count of items added to the trie.
    /// </summary>
    public int Count => _patterns.Count;

    // TODO: Find a way to actually lock the node so it cannot be modified anymore.
    // I would prefer not to add a flag to all nodes which would only affect the root node.
    /// <summary>
    /// Freezes the Trie and returns a read-only variant.
    /// </summary>
    /// <returns></returns>
    public ReadableTrie<TKey, TResult> Freeze() => new ReadableTrie<TKey, TResult>(_root, _patterns);

    /// <summary>
    /// Adds a new item to the Trie with the given key pattern.
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public InsertableTrie<TKey, TResult> Add(IEnumerable<TKey> keys, TResult result)
    {
        Assert.ArgumentNotNull(keys, nameof(keys));
        Assert.ArgumentNotNull(result, nameof(result));
        var current = _root;
        var keyList = keys.ToArray();
        foreach (var key in keyList)
            current = current.GetOrAddChild(key);

        if (current.TryAddResult(result))
        {
            _patterns.Add(keyList);
            _root.SetPatternDepth(keyList.Length);
        }

        return this;
    }
}

// TODO: Want to move this into Internal, including the Get extension method below.
// All tests which use InsertableTrie and ReadableTrie directly should be converted to tests
// on the publicly-supported Trie parser.
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
