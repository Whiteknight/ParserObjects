using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ParserObjects.Internal.Sequences;

public sealed class DelegateCharSequence : ICharSequence
{
    private readonly Func<int, (char next, bool atEnd)> _function;
    private readonly SequenceOptions<char> _options;
    private readonly Buffer[] _buffers;

    private WorkingSequenceStatistics _stats;
    private int _bufferPtr;
    private int _index;
    private int _line;
    private int _column;
    private int _endIndex;

    public DelegateCharSequence(Func<int, (char next, bool atEnd)> function, SequenceOptions<char> options)
    {
        _function = function;
        _options = options;
        _options.Validate();
        _index = 0;
        _line = 1;
        _column = 0;
        _endIndex = -1;
        _buffers = new Buffer[2]
        {
            new Buffer(_options.BufferSize, 0),
            new Buffer(_options.BufferSize, -1)
        };
        GetNextRaw(false);
        _bufferPtr = 0;
        _stats = default;
    }

    private struct Buffer
    {
        public Buffer(int size, int startIndex)
        {
            Values = new char[size];
            StartIndex = startIndex;
            HighWaterMark = -1;
        }

        public char[] Values { get; }
        public int StartIndex { get; set; }
        public int HighWaterMark { get; set; }
    }

    public Location CurrentLocation => new Location(string.Empty, _line, _column);

    public bool IsAtEnd => _endIndex > 0 && _index >= _endIndex;

    public int Consumed => _index;

    public char GetNext()
    {
        if (IsAtEnd)
            return _options.EndSentinel;

        var next = GetNextRaw(true);
        _stats.ItemsRead++;
        if (next == '\r')
        {
            var lookahead = GetNextRaw(false);
            if (lookahead == '\n')
                _index++;
            next = '\n';
        }

        if (next == '\n')
        {
            _line++;
            _column = 0;
            return next;
        }

        _column++;
        return next;
    }

    public char Peek()
    {
        var next = GetNextRaw(false);
        if (next == '\r')
            next = '\n';

        _stats.ItemsPeeked++;
        return next;
    }

    private char GetNextRaw(bool advance)
    {
        if (IsAtEnd)
            return _options.EndSentinel;

        // Check to see if the character to read is in the current buffer. If so, return it directly
        var buffer = _buffers[_bufferPtr];
        var bufferOffset = _index - buffer.StartIndex;
        if (bufferOffset > 0 && bufferOffset < buffer.HighWaterMark)
        {
            var value = buffer.Values[bufferOffset];
            if (advance)
                AdvanceIndex();
            return value;
        }

        // Get the next value
        var (next, atEnd) = _function(_index);
        _stats.ItemsGenerated++;
        if (atEnd)
            _endIndex = _index + 1;

        // Add it to the current buffer. bufferOffset should be inside the current buffer because
        // we would have already advanced the index during the previous GetNext()
        buffer.Values[bufferOffset] = next;
        _buffers[_bufferPtr].HighWaterMark = bufferOffset;

        if (advance)
            AdvanceIndex();

        return next;
    }

    private void AdvanceIndex()
    {
        _index++;
        var nextBufferOffset = _index - _buffers[_bufferPtr].StartIndex;
        if (nextBufferOffset >= _options.BufferSize)
        {
            // The next index is beyond the current buffer. So we swap over to the next buffer
            _bufferPtr = (_bufferPtr + 1) % 2;
            _buffers[_bufferPtr].StartIndex = _index;
            _buffers[_bufferPtr].HighWaterMark = -1;
            _stats.BufferFills++;
        }
    }

    public string GetRemainder()
    {
        if (IsAtEnd)
            return string.Empty;

        var cp = Checkpoint();

        // If we know where the end is, we can allocate an array and go get it
        if (_endIndex >= 0)
        {
            var approxSize = _endIndex - _index;
            var buffer = new char[approxSize];
            int i = 0;
            while (!IsAtEnd)
                buffer[i++] = GetNext();
            cp.Rewind();
            return new string(buffer, 0, i);
        }

        // Else we need a list to dynamically grow until we find the end
        var list = new List<char>();
        while (!IsAtEnd)
            list.Add(GetNext());

        cp.Rewind();
        var chars = list.ToArray();
        return new string(chars);
    }

    public SequenceStatistics GetStatistics()
        => _stats.Snapshot();

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _index, _index, _buffers[_bufferPtr].StartIndex, new Location(string.Empty, _line, _column));
    }

    public char[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end)
    {
        if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
            return Array.Empty<char>();

        var size = end.Consumed - start.Consumed;
        var chars = new char[size];
        var cp = Checkpoint();
        start.Rewind();
        for (int i = 0; i < size; i++)
            chars[i] = GetNext();
        cp.Rewind();
        return chars;
    }

    public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
        => new string(GetBetween(start, end));

    public bool Owns(SequenceCheckpoint checkpoint)
        => checkpoint.Sequence == this;

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        var bufferStartIndex = (int)checkpoint.StreamPosition;

        _index = checkpoint.Index;
        _line = checkpoint.Location.Line;
        _column = checkpoint.Location.Column;
        _stats.Rewinds++;

        // Checkpoint is at the end position, so we don't need to do anything with buffers
        if (_endIndex > 0 && _index >= _endIndex)
        {
            _stats.RewindsToCurrentBuffer++;
            return;
        }

        var bufferIndex = checkpoint.Index - bufferStartIndex;
        if (_buffers[_bufferPtr].StartIndex == bufferStartIndex)
        {
            _stats.RewindsToCurrentBuffer++;
            MakeSureBufferIsFilledToRequestedIndex(bufferIndex);
            return;
        }

        // At this point it's not the current buffer, so we're switching regardless. If it's in the
        // other buffer we can just use it as-is. Otherwise refill the other buffer from the beginning
        _bufferPtr = (_bufferPtr + 1) % 2;
        if (_buffers[_bufferPtr].StartIndex == bufferStartIndex)
        {
            MakeSureBufferIsFilledToRequestedIndex(bufferIndex);
            return;
        }

        _buffers[_bufferPtr].StartIndex = bufferStartIndex;
        _buffers[_bufferPtr].HighWaterMark = -1;
        MakeSureBufferIsFilledToRequestedIndex(bufferIndex);
    }

    private void MakeSureBufferIsFilledToRequestedIndex(int bufferIndex)
    {
        // If we are below the high water mark we can just set index. Otherwise we need to
        // reset the whole buffer read again from the beginning

        for (int i = _buffers[_bufferPtr].HighWaterMark + 1; i <= bufferIndex; i++)
        {
            Debug.Assert(_endIndex == -1 || i < _endIndex, "We should not have a checkpoint beyond the end");

            var (next, atEnd) = _function(i);
            _stats.ItemsGenerated++;
            _buffers[_bufferPtr].Values[i] = next;
            if (atEnd && _endIndex == -1)
            {
                _endIndex = i + 1;
                break;
            }
        }

        // At this point we are pointing into the current buffer and have made to to have read
        // all the way to the requested index value, so we are done
    }

    public void Reset()
    {
        _stats.Rewinds++;
        _index = 0;
        _line = 1;
        _column = 0;
    }
}
