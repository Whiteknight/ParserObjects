using System;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence decorator which takes items from the input sequence and transforms them. Notice
/// that when using MapSequence, you should not directly access the underlying sequence
/// anymore. Data may be lost, because items put back to the MapSequence cannot be un-mapped
/// and put back to the underlying sequence.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class MapSequence<TInput, TOutput> : ISequence<TOutput>
{
    private readonly ISequence<TInput> _inputs;
    private readonly Func<TInput, TOutput> _map;

    public MapSequence(ISequence<TInput> inputs, Func<TInput, TOutput> map)
    {
        Assert.ArgumentNotNull(inputs);
        Assert.ArgumentNotNull(map);
        _inputs = inputs;
        _map = map;
    }

    public TOutput GetNext() => GetNext(true);

    public TOutput Peek() => GetNext(false);

    public Location CurrentLocation => _inputs.CurrentLocation;

    public bool IsAtEnd => _inputs.IsAtEnd;

    public SequencePositionFlags Flags => _inputs.Flags;

    public int Consumed => _inputs.Consumed;

    public SequenceCheckpoint Checkpoint() => _inputs.Checkpoint();

    private TOutput GetNext(bool advance)
    {
        if (!advance)
        {
            var peek = _inputs.Peek();
            return _map(peek);
        }

        var next = _inputs.GetNext();
        return _map(next);
    }

    public SequenceStatistics GetStatistics() => _inputs.GetStatistics();

    public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<TOutput, TData, TResult> map)
    {
        return _inputs.GetBetween(start, end, (data, _map, map), static (b, d) =>
        {
            var (data, itemMap, resultMap) = d;
            var mappedItems = new TOutput[b.Length];
            for (int i = 0; i < b.Length; i++)
                mappedItems[i] = itemMap(b[i]);
            return resultMap(mappedItems.AsSpan(), data);
        });
    }

    public bool Owns(SequenceCheckpoint checkpoint) => _inputs.Owns(checkpoint);

    public void Rewind(SequenceCheckpoint checkpoint) => _inputs.Rewind(checkpoint);

    public void Reset() => _inputs.Reset();
}
