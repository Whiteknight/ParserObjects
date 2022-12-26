using System;
using System.Linq;
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

    public FilterSequence(ISequence<T> inputs, Func<T, bool> predicate)
    {
        Assert.ArgumentNotNull(inputs, nameof(inputs));
        Assert.ArgumentNotNull(predicate, nameof(predicate));
        _inputs = inputs;
        _predicate = predicate;
    }

    public T GetNext()
    {
        bool hasMore = DiscardNonMatches();

        // If we don't have any values, we are at the end. In this case calling
        // _inputs.GetNext() will return the end sentinel, which is not subject to filtering.
        if (!hasMore)
            return _inputs.GetNext();

        return _inputs.GetNext();
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

    public int Consumed => _inputs.Consumed;

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

    public SequenceCheckpoint Checkpoint()
        => _inputs.Checkpoint();

    public SequenceStatistics GetStatistics() => _inputs.GetStatistics();

    public T[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end)
    {
        var result = _inputs.GetBetween(start, end);
        return result.Where(_predicate).ToArray();
    }

    public bool Owns(SequenceCheckpoint checkpoint) => _inputs.Owns(checkpoint);

    public void Rewind(SequenceCheckpoint checkpoint) => _inputs.Rewind(checkpoint);

    public void Reset() => _inputs.Reset();
}
