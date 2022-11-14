using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Sequences;

/// <summary>
/// Adaptor to represent a sequence as an indexable buffer like an array or list. Care must
/// be taken to ensure unused items from the buffer are returned to the sequence after work
/// on the buffer has completed.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SequenceBuffer<T>
{
    private const int DefaultMaxItems = 128;
    private readonly ISequence<T> _input;
    private readonly int _maxItems;
    private readonly List<(T value, ISequenceCheckpoint cont)> _buffer;
    private readonly int _offset;
    private readonly ISequenceCheckpoint _startCheckpoint;

    public SequenceBuffer(ISequence<T> input, int maxItems = DefaultMaxItems)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        _input = input;
        _maxItems = maxItems <= 0 ? DefaultMaxItems : maxItems;
        _buffer = new List<(T value, ISequenceCheckpoint cont)>();
        _offset = 0;
        _startCheckpoint = _input.Checkpoint();
    }

    private SequenceBuffer(ISequence<T> input, List<(T value, ISequenceCheckpoint cont)> buffer, int offset, ISequenceCheckpoint startCheckpoint, int maxItems)
    {
        _input = input;
        _buffer = buffer;
        _offset = offset;
        _startCheckpoint = startCheckpoint;
        _maxItems = maxItems;
    }

    public SequenceBuffer<T> CopyFrom(int i) => new SequenceBuffer<T>(_input, _buffer, i, _startCheckpoint, _maxItems);

    public T[] Capture(int i)
    {
        if (i <= 0)
        {
            _startCheckpoint.Rewind();
            return Array.Empty<T>();
        }

        _buffer[i - 1].cont.Rewind();
        return _buffer.Skip(_offset).Take(i).Select(v => v.value).ToArray();
    }

    public T? this[int index]
    {
        get
        {
            var realIndex = _offset + index;
            FillUntil(realIndex);
            if (realIndex >= _buffer.Count)
                return default;
            return _buffer[realIndex].value;
        }
    }

    public bool IsPastEnd(int index)
    {
        var realIndex = _offset + index;
        FillUntil(realIndex);
        return realIndex >= _buffer.Count;
    }

    private void FillUntil(int i)
    {
        if (i > _maxItems)
            i = _maxItems;

        if (_buffer.Count > i)
            return;

        int numToGet = _buffer.Count - i + 1;
        for (var j = 0; j < numToGet; j++)
        {
            if (_input.IsAtEnd)
                break;
            var value = _input.GetNext();
            var cont = _input.Checkpoint();
            _buffer.Add((value, cont));
        }
    }
}
