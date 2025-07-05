using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ParserObjects.Internal.Sequences;

public static class UserDelegate
{
    private struct Buffer<T>
    {
        public Buffer(int size, int startIndex)
        {
            Values = new T[size];
            StartIndex = startIndex;
            HighWaterMark = -1;
        }

        public T[] Values { get; }

        public int StartIndex { get; set; }

        public int HighWaterMark { get; set; }
    }

    private struct InternalSequence<T>
    {
        private readonly Func<int, (T next, bool atEnd)> _function;
        private readonly SequenceOptions<T> _options;
        private readonly Buffer<T>[] _buffers;

        private WorkingSequenceStatistics _stats;
        private int _bufferPtr;
        private int _index;
        private int _endIndex;

        public InternalSequence(Func<int, (T next, bool atEnd)> function, SequenceOptions<T> options)
            : this()
        {
            _function = function;
            _options = options;
            _options.Validate();
            _buffers = [
                new Buffer<T>(_options.BufferSize, 0),
                new Buffer<T>(_options.BufferSize, -1)
            ];
            _stats = default;
            _bufferPtr = 0;
            _index = 0;
            _endIndex = -1;
            Flags = SequenceStateTypes.StartOfInput;

            GetNextRaw(false);
        }

        public SequenceOptions<T> Options => _options;

        public SequenceStatistics GetStatistics() => _stats.Snapshot();

        public readonly int Index => _index;
        public readonly int EndIndex => _endIndex;

        public readonly bool IsAtEnd => _endIndex >= 0 && _index >= _endIndex;

        public SequenceStateTypes Flags { get; private set; }

        public T GetNext()
        {
            if (IsAtEnd)
            {
                Flags = Flags.With(SequenceStateTypes.EndOfInput);
                return _options.EndSentinel;
            }

            var next = GetNextRaw(true);
            Flags = Flags.Without(SequenceStateTypes.StartOfInput);
            _stats.ItemsRead++;
            if (IsAtEnd)
                Flags = Flags.With(SequenceStateTypes.EndOfInput);

            return next;
        }

        public T Peek()
        {
            var next = GetNextRaw(false);
            _stats.ItemsPeeked++;
            return next;
        }

        private void RecordStatsAndAdvanceIndex(bool advance)
        {
            if (!advance)
            {
                _stats.ItemsPeeked++;
                return;
            }

            _stats.ItemsRead++;
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

        public T GetNextRaw(bool advance)
        {
            if (IsAtEnd)
                return _options.EndSentinel;

            // Check to see if the character to read is in the current buffer. If so, return it directly
            var buffer = _buffers[_bufferPtr];
            var bufferOffset = _index - buffer.StartIndex;
            if (bufferOffset > 0 && bufferOffset < buffer.HighWaterMark)
            {
                var value = buffer.Values[bufferOffset];
                RecordStatsAndAdvanceIndex(advance);
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

            RecordStatsAndAdvanceIndex(advance);
            return next;
        }

        public SequenceCheckpoint Checkpoint(ISequence sequence)
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(sequence, _index, _index, _buffers[_bufferPtr].StartIndex, Flags, new Location(string.Empty, 1, _index));
        }

        public SequenceCheckpoint Checkpoint(ISequence sequence, int line, int column)
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(sequence, _index, _index, _buffers[_bufferPtr].StartIndex, Flags, new Location(string.Empty, line, column));
        }

        public TResult GetBetween<TData, TResult>(ISequence sequence, SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<T, TData, TResult> map)
        {
            var size = end.Consumed - start.Consumed;
            var chars = new T[size];
            var cp = Checkpoint(sequence);
            start.Rewind();
            for (int i = 0; i < size; i++)
                chars[i] = GetNext();
            cp.Rewind();
            return map(chars.AsSpan(), data);
        }

        public void Rewind(SequenceCheckpoint checkpoint)
        {
            var bufferStartIndex = (int)checkpoint.StreamPosition;
            Flags = checkpoint.Flags;

            _index = checkpoint.Index;
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
                if (atEnd)
                {
                    Debug.Assert(_endIndex >= 0, "Since this is a rewind we should already have seen the end index");
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
            Flags = SequenceStateTypes.StartOfInput;
        }
    }

    public sealed class Sequence<T> : ISequence<T>
    {
        private InternalSequence<T> _internal;

        public Sequence(Func<int, (T next, bool atEnd)> function, SequenceOptions<T> options)
        {
            _internal = new InternalSequence<T>(function, options);
        }

        public Location CurrentLocation => new Location(string.Empty, 1, _internal.Index);

        public bool IsAtEnd => _internal.IsAtEnd;

        public SequenceStateTypes Flags => _internal.Flags;

        public int Consumed => _internal.Index;

        public T GetNext() => _internal.GetNext();

        public T Peek() => _internal.Peek();

        public SequenceStatistics GetStatistics()
            => _internal.GetStatistics();

        public SequenceCheckpoint Checkpoint() => _internal.Checkpoint(this);

        public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<T, TData, TResult> map)
        {
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return map([], data);

            return _internal.GetBetween(this, start, end, data, map);
        }

        public bool Owns(SequenceCheckpoint checkpoint)
            => checkpoint.Sequence == this;

        public void Rewind(SequenceCheckpoint checkpoint)
            => _internal.Rewind(checkpoint);

        public void Reset() => _internal.Reset();
    }

    public sealed class CharSequence : ICharSequence
    {
        private InternalSequence<char> _internal;
        private int _line;
        private int _column;
        private SequenceStateTypes _flags;

        public CharSequence(Func<int, (char next, bool atEnd)> function, SequenceOptions<char> options)
        {
            _internal = new InternalSequence<char>(function, options);
            _line = 1;
            _column = 0;
            _flags = SequenceStateTypes.StartOfLine;
        }

        public Location CurrentLocation => new Location(string.Empty, _line, _column);

        public bool IsAtEnd => _internal.IsAtEnd;

        public int Consumed => _internal.Index;

        public SequenceStateTypes Flags => _internal.Flags | _flags;

        public char GetNext()
        {
            if (_internal.IsAtEnd)
                return _internal.Options.EndSentinel;

            var next = _internal.GetNextRaw(true);
            if (next == '\r')
            {
                var lookahead = _internal.GetNextRaw(false);
                if (lookahead == '\n')
                    _internal.GetNextRaw(true);
                next = '\n';
            }

            if (next == '\n')
            {
                _line++;
                _column = 0;
                _flags = _flags.With(SequenceStateTypes.StartOfLine);
                return next;
            }

            _flags = _flags.Without(SequenceStateTypes.StartOfLine);
            _column++;
            return next;
        }

        public char Peek()
        {
            var next = _internal.GetNextRaw(false);
            if (next == '\r')
                next = '\n';

            return next;
        }

        public string GetRemainder()
        {
            if (IsAtEnd)
                return string.Empty;

            var cp = Checkpoint();

            // If we know where the end is, we can allocate an array and go get it
            if (_internal.EndIndex >= 0)
            {
                var approxSize = _internal.EndIndex - _internal.Index;
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
            => _internal.GetStatistics();

        public SequenceCheckpoint Checkpoint()
            => _internal.Checkpoint(this, _line, _column);

        public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<char, TData, TResult> map)
        {
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return map([], data);

            return _internal.GetBetween(this, start, end, data, map);
        }

        public bool Owns(SequenceCheckpoint checkpoint)
            => checkpoint.Sequence == this;

        public void Rewind(SequenceCheckpoint checkpoint)
        {
            _flags = checkpoint.Flags.Only(SequenceStateTypes.StartOfLine);
            _internal.Rewind(checkpoint with
            {
                Flags = checkpoint.Flags.Without(SequenceStateTypes.StartOfLine)
            });
        }

        public void Reset()
        {
            _internal.Reset();
            _line = 1;
            _column = 0;
            _flags = SequenceStateTypes.StartOfLine;
        }
    }
}
