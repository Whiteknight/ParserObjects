using System;

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
        Assert.ArgumentNotNull(inputs);
        Assert.ArgumentNotNull(predicate);
        _inputs = inputs;
        _predicate = predicate;
        DiscardNonMatches();
    }

    public T GetNext()
    {
        var item = _inputs.GetNext();
        DiscardNonMatches();
        return item;
    }

    public T Peek() => _inputs.Peek();

    public Location CurrentLocation => _inputs.CurrentLocation;

    public bool IsAtEnd => _inputs.IsAtEnd;

    public SequenceStateTypes Flags => _inputs.Flags;

    public int Consumed => _inputs.Consumed;

    private void DiscardNonMatches()
    {
        while (true)
        {
            if (_inputs.IsAtEnd)
                return;
            var next = _inputs.Peek();
            if (_predicate(next))
                return;
            _inputs.GetNext();
        }
    }

    public SequenceCheckpoint Checkpoint()
        => _inputs.Checkpoint();

    public SequenceStatistics GetStatistics() => _inputs.GetStatistics();

    public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<T, TData, TResult> map)
    {
        return _inputs.GetBetween(start, end, (data, _predicate, map), static (b, d) =>
        {
            var (data, predicate, map) = d;
            var buffer = new T[b.Length];
            int j = 0;
            for (int i = 0; i < b.Length; i++)
            {
                var c = b[i];
                if (predicate(c))
                    buffer[j++] = c;
            }

            return map(buffer.AsSpan(0, j), data);
        });
    }

    public bool Owns(SequenceCheckpoint checkpoint) => _inputs.Owns(checkpoint);

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _inputs.Rewind(checkpoint);
        DiscardNonMatches();
    }

    public void Reset()
    {
        _inputs.Reset();
        DiscardNonMatches();
    }
}
