using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

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
        Assert.ArgumentNotNull(enumerable, nameof(enumerable));
        _list = enumerable is IReadOnlyList<T> list ? list : enumerable.ToArray();
        _endSentinelValue = endSentinel;

        _index = 0;

        _stats = default;
    }

    public ListSequence(IReadOnlyList<T> list, T endSentinel)
    {
        Assert.ArgumentNotNull(list, nameof(list));
        _list = list;
        _endSentinelValue = endSentinel;

        _index = 0;

        _stats = default;
    }

    private class Node
    {
        public T? Value { get; set; }
        public Node? Next { get; set; }
    }

    // Notice that if T == char, GetNext() here doesn't respect normalized line endings, line
    // counting, etc.

    public T GetNext()
    {
        if (_index >= _list.Count)
            return _endSentinelValue;

        var value = _list[_index];
        _index++;

        _stats.ItemsRead++;
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

    public int Consumed => _index;

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _index, _index, 0L, CurrentLocation);
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _stats.Rewinds++;
        _index = checkpoint.Index;
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;

    public T[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end)
    {
        if (!Owns(start) || !Owns(end))
            return Array.Empty<T>();

        if (start.CompareTo(end) >= 0)
            return Array.Empty<T>();

        var buffer = new T[end.Consumed - start.Consumed];
        for (int i = 0; i < end.Consumed - start.Consumed; i++)
            buffer[i] = _list[start.Consumed + i];

        return buffer;
    }
}
