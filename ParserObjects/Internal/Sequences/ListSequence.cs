using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static ParserObjects.Internal.Sequences.SequenceFlags;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// Wraps an IEnumerable as an ISequence. Makes the items from the enumerable usable in parse
/// operations.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ListSequence<T> : ISequence<T>
{
    private readonly IReadOnlyList<T> _list;
    private readonly T _endSentinelValue;

    private WorkingSequenceStatistics _stats;
    private int _index;

    public ListSequence(IEnumerable<T> enumerable, T endSentinel)
    {
        Assert.NotNull(enumerable);
        _list = enumerable is IReadOnlyList<T> list ? list : enumerable.ToArray();
        _endSentinelValue = endSentinel;
        _stats = default;
        Reset();
    }

    public ListSequence(IReadOnlyList<T> list, T endSentinel)
    {
        Assert.NotNull(list);
        _list = list;
        _endSentinelValue = endSentinel;
        _stats = default;
        Reset();
    }

    // Notice that if T == char, GetNext() here doesn't respect normalized line endings, line
    // counting, etc.

    public T GetNext()
    {
        if (_index >= _list.Count)
            return _endSentinelValue;

        T value = _list[_index];
        _index++;

        _stats.ItemsRead++;
        Flags = Flags.Without(SequenceStateTypes.StartOfInput);
        if (_index >= _list.Count)
            Flags = Flags.With(SequenceStateTypes.EndOfInput);
        return value;
    }

    public T Peek()
    {
        if (_index >= _list.Count)
            return _endSentinelValue;

        return _list[_index];
    }

    public Location CurrentLocation => new Location(string.Empty, 1, _index);

    public bool IsAtEnd => _index >= _list.Count;

    public SequenceStateTypes Flags { get; private set; }

    public int Consumed => _index;

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _index, _index, 0L, Flags, CurrentLocation);
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _stats.Rewinds++;
        _stats.RewindsToCurrentBuffer++;
        _index = checkpoint.Index;
        Flags = checkpoint.Flags;
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;

    public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<T, TData, TResult> map)
    {
        if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
            return map(ReadOnlySpan<T>.Empty, data);

        var size = end.Consumed - start.Consumed;
        return _list switch
        {
            List<T> l => map(CollectionsMarshal.AsSpan(l).Slice(start.Consumed, size), data),
            T[] a => map(a.AsSpan(start.Consumed, size), data),
            IReadOnlyList<T> ro => map(ro.ToArray().AsSpan(start.Consumed, size), data),
        };
    }

    public void Reset()
    {
        _index = 0;
        Flags = FlagsForStartOfSequence(_list.Count == 0);
    }
}
