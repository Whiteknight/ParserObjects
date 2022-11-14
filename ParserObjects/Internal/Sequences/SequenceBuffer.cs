using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// Adaptor to represent a sequence as an indexable buffer like an array or list. Care must
/// be taken to ensure unused items from the buffer are returned to the sequence after work
/// on the buffer has completed.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SequenceBuffer<T>
{
    private const int _defaultMaxItems = 128;
    private readonly ISequence<T> _input;
    private readonly int _maxItems;
    private readonly List<(T value, ISequenceCheckpoint cont)> _buffer;
    private readonly int _offset;
    private readonly ISequenceCheckpoint _startCheckpoint;

    public SequenceBuffer(ISequence<T> input, int maxItems = _defaultMaxItems)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        _input = input;
        _maxItems = maxItems <= 0 ? _defaultMaxItems : maxItems;
        _buffer = new List<(T value, ISequenceCheckpoint cont)>();
        _offset = 0;
        _startCheckpoint = _input.Checkpoint();
    }

    private SequenceBuffer(SequenceBuffer<T> parent, int offset)
    {
        _input = parent._input;
        _buffer = parent._buffer;
        _offset = offset;
        _startCheckpoint = parent._startCheckpoint;
        _maxItems = parent._maxItems;
    }

    public SequenceBuffer<T> CopyFrom(int i) => new SequenceBuffer<T>(this, i);

    public T[] Capture(int i)
    {
        if (i <= 0)
        {
            _startCheckpoint.Rewind();
            return Array.Empty<T>();
        }

        _buffer[i - 1].cont.Rewind();
        var result = new T[i];
        for (int index = 0; index < i; index++)
            result[index] = _buffer[_offset + index].value;
        return result;
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
