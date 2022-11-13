using System;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// Filter a sequence to only return items which match a predicate.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class FilterSequence<T> : ISequence<T>
{
    private readonly ISequence<T> _inputs;
    private readonly Func<T, bool> _predicate;

    private int _consumed;

    public FilterSequence(ISequence<T> inputs, Func<T, bool> predicate)
    {
        Assert.ArgumentNotNull(inputs, nameof(inputs));
        Assert.ArgumentNotNull(predicate, nameof(predicate));
        _inputs = inputs;
        _predicate = predicate;
        _consumed = 0;
    }

    public T GetNext()
    {
        bool hasMore = DiscardNonMatches();

        // If we don't have any values, we are at the end. In this case calling
        // _inputs.GetNext() will return the end sentinel, which is not subject to filtering.
        if (!hasMore)
            return _inputs.GetNext();

        var result = _inputs.GetNext();
        _consumed++;
        return result;
    }

    public T Peek()
    {
        DiscardNonMatches();
        return _inputs.Peek();
    }

    public Location CurrentLocation => _inputs.CurrentLocation;

    public bool IsAtEnd
    {
        get
        {
            DiscardNonMatches();
            return _inputs.IsAtEnd;
        }
    }

    public int Consumed => _consumed;

    private bool DiscardNonMatches()
    {
        while (true)
        {
            if (_inputs.IsAtEnd)
                return false;
            var next = _inputs.Peek();
            if (_predicate(next))
                return true;
            _inputs.GetNext();
        }
    }

    public ISequenceCheckpoint Checkpoint()
        => _inputs.Checkpoint();

    public ISequenceStatistics GetStatistics() => _inputs.GetStatistics();
}
