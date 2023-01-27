using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Tries;

public struct ReadableTrie<TKey, TResult>
{
    private readonly Node<TKey, TResult> _root;
    private readonly IReadOnlyList<IReadOnlyList<TKey>> _patterns;

    private ReadableTrie(Node<TKey, TResult> root, IReadOnlyList<IReadOnlyList<TKey>> patterns)
    {
        _root = root;
        _patterns = patterns;
    }

    public static ReadableTrie<TKey, TResult> Create(InsertableTrie<TKey, TResult> trie)
    {
        var (root, patterns) = trie;
        return new ReadableTrie<TKey, TResult>(root, patterns);
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
