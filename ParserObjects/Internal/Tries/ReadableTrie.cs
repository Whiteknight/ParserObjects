using System.Collections.Generic;

namespace ParserObjects.Internal.Tries;

public readonly struct ReadableTrie<TKey, TResult>
{
    private readonly RootNode<TKey, TResult> _root;
    private readonly IReadOnlyList<IReadOnlyList<TKey>> _patterns;

    private ReadableTrie(RootNode<TKey, TResult> root, IReadOnlyList<IReadOnlyList<TKey>> patterns)
    {
        _root = root;
        _patterns = patterns;
    }

    public static ReadableTrie<TKey, TResult> Create(InsertableTrie<TKey, TResult> trie)
    {
        var (root, patterns) = trie;
        root.Lock();
        return new ReadableTrie<TKey, TResult>(root, patterns);
    }

    public PartialResult<TResult> Get(ISequence<TKey> keys)
    {
        Assert.NotNull(keys);
        return _root.Get(keys);
    }

    public bool CanGet(ISequence<TKey> keys)
    {
        Assert.NotNull(keys);
        return _root.CanGet(keys);
    }

    public IReadOnlyList<Alternative<TResult>> GetMany(ISequence<TKey> keys)
    {
        Assert.NotNull(keys);
        return _root.GetMany(keys);
    }

    public IEnumerable<IReadOnlyList<TKey>> GetAllPatterns() => _patterns;
}
