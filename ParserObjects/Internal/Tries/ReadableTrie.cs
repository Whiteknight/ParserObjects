using System.Collections.Generic;
using static ParserObjects.Internal.Assert;

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
        => _root.Get(NotNull(keys));

    public bool CanGet(ISequence<TKey> keys)
        => _root.CanGet(NotNull(keys));

    public IReadOnlyList<Alternative<TResult>> GetMany(ISequence<TKey> keys)
        => _root.GetMany(NotNull(keys));

    public IEnumerable<IReadOnlyList<TKey>> GetAllPatterns() => _patterns;
}
