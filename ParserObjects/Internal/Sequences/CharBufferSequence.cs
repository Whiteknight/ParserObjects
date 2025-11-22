using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using static ParserObjects.Internal.Assert;
using static ParserObjects.Internal.Sequences.SequenceFlags;

namespace ParserObjects.Internal.Sequences;

public static class CharBufferSequence
{
    /// <summary>
    /// A sequence of characters read from a string. Does not do any normalization of line endings.
    /// This type is an optimization for cases where we have a string and we explicitly do not need
    /// normalization. We can save a lot of up-front work in that case and store the string as-is.
    /// </summary>
    public sealed class FromNonnormalizedString : ICharSequence
    {
        private readonly SequenceOptions<char> _options;
        private readonly string _data;

        private WorkingSequenceStatistics _stats;
        private int _index;
        private int _line;
        private int _column;
        private SequenceStateTypes _flags;

        public FromNonnormalizedString(string s, SequenceOptions<char> options)
        {
            Debug.Assert(options.MaintainLineEndings);
            _options = options.Validate();
            _data = NotNull(s);
            Length = s.Length;
            _stats = default;
            _index = 0;
            _line = 1;
            _column = 0;
            _flags = FlagsForStartOfCharSequence(Length == 0);
        }

        public char GetNext()
        {
            if (IsAtEnd)
                return _options.EndSentinel;
            var next = GetNextInternal(true);

            _flags &= ~SequenceStateTypes.StartOfInput;
            _stats.ItemsRead++;

            if (_index >= Length)
                _flags |= SequenceStateTypes.EndOfInput;

            if (next == '\n')
            {
                _line++;
                _column = 0;
                _flags |= SequenceStateTypes.StartOfLine;
                return next;
            }

            _flags &= ~SequenceStateTypes.StartOfLine;

            _column++;
            return next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char GetNextInternal(bool advance)
        {
            if (_index >= Length)
                return _options.EndSentinel;
            var value = _data[_index];
            if (advance)
                _index++;
            return value;
        }

        public char Peek()
        {
            var next = GetNextInternal(false);
            _stats.ItemsPeeked++;
            return next;
        }

        public Location CurrentLocation => new Location(_options.FileName, _line, _column);

        public bool IsAtEnd => _index >= Length;

        public SequenceStateTypes Flags => _flags;

        public int Consumed => _index;

        // Keep a Data accessor to preserve original behavior
        public string Data => _data;

        public int Length { get; }

        public string GetRemainder()
        {
            if (_index == 0)
                return _data;
            if (_index >= Length)
                return string.Empty;
            return _data.Substring(_index);
        }

        public void Reset()
        {
            _index = 0;
            _line = 1;
            _column = 0;
            _flags = FlagsForStartOfCharSequence(Length == 0);
        }

        public SequenceCheckpoint Checkpoint()
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(this, _index, _index, 0L, _flags, new Location(_options.FileName, _line, _column));
        }

        public void Rewind(SequenceCheckpoint checkpoint)
        {
            _stats.Rewinds++;
            _stats.RewindsToCurrentBuffer++;
            _index = checkpoint.Index;
            _line = checkpoint.Location.Line;
            _column = checkpoint.Location.Column;
            _flags = checkpoint.Flags;
        }

        public SequenceStatistics GetStatistics() => _stats.Snapshot();

        public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<char, TData, TResult> map)
        {
            NotNull(map);
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return map([], data);

            var size = end.Consumed - start.Consumed;
            var span = _data.AsSpan(start.Consumed, size);
            return map(span, data);
        }

        public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
        {
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return string.Empty;

            int size = end.Consumed - start.Consumed;
            return _data.Substring(start.Consumed, size);
        }

        public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
    }

    /// <summary>
    /// A sequence of characters stored in an in-memory array. May or may not normalize line endings.
    /// </summary>
    public sealed class FromCharArray : ICharSequence
    {
        // Inlined InternalStateCharArray fields to avoid an extra indirection and make the hot path JIT-friendlier.
        private readonly SequenceOptions<char> _options;
        private readonly char[] _data;

        private WorkingSequenceStatistics _stats;
        private int _index;
        private int _line;
        private int _column;
        private SequenceStateTypes _flags;

        public FromCharArray(string s, SequenceOptions<char> options)
        {
            NotNull(s);
            (var buffer, int bufferLength) = Normalize(s, options.NormalizeLineEndings);
            _options = options.Validate();
            _data = buffer;
            Length = bufferLength;
            _stats = default;
            _index = 0;
            _line = 1;
            _column = 0;
            _flags = FlagsForStartOfCharSequence(Length == 0);
        }

        public FromCharArray(IReadOnlyList<char> s, SequenceOptions<char> options)
        {
            NotNull(s);
            (var buffer, int bufferLength) = Normalize(s, options.NormalizeLineEndings);
            _options = options.Validate();
            _data = buffer;
            Length = bufferLength;
            _stats = default;
            _index = 0;
            _line = 1;
            _column = 0;
            _flags = FlagsForStartOfCharSequence(Length == 0);
        }

        private static (char[] Buffer, int Length) Normalize(string s, bool normalize)
        {
            Debug.Assert(normalize);

            if (s.Length == 0)
                return (Array.Empty<char>(), 0);
            if (s.Length == 1)
                return (new[] { s[0] == '\r' ? '\n' : s[0] }, 1);

            var chars = new char[s.Length];

            int destIdx = 0;
            int srcIdx = 0;
            for (; srcIdx < s.Length - 1; srcIdx++)
            {
                // \r -> \n
                // \r\n -> \n
                var c = s[srcIdx];
                if (c != '\r')
                {
                    chars[destIdx++] = c;
                    continue;
                }

                var next = s[srcIdx + 1];
                if (next == '\n')
                {
                    chars[destIdx++] = '\n';
                    srcIdx++;
                    continue;
                }

                chars[destIdx++] = '\n';
            }

            if (srcIdx < s.Length)
            {
                var c = s[srcIdx];
                chars[destIdx++] = c == '\r' ? '\n' : c;
            }

            return (chars, destIdx);
        }

        private static (char[] Buffer, int Length) Normalize(IReadOnlyList<char> s, bool normalize)
        {
            if (!normalize)
                return (s.ToArray(), s.Count);

            if (s.Count == 0)
                return (Array.Empty<char>(), 0);
            if (s.Count == 1)
                return (new[] { s[0] == '\r' ? '\n' : s[0] }, 1);

            var chars = new char[s.Count];

            int destIdx = 0;
            int srcIdx = 0;
            for (; srcIdx < s.Count - 1; srcIdx++)
            {
                // \r -> \n
                // \r\n -> \n
                var c = s[srcIdx];
                if (c != '\r')
                {
                    chars[destIdx++] = c;
                    continue;
                }

                var next = s[srcIdx + 1];
                if (next == '\n')
                {
                    chars[destIdx++] = '\n';
                    srcIdx++;
                    continue;
                }

                chars[destIdx++] = '\n';
            }

            if (srcIdx < s.Count)
            {
                var c = s[srcIdx];
                chars[destIdx++] = c == '\r' ? '\n' : c;
            }

            return (chars, destIdx);
        }

        public char GetNext()
        {
            if (IsAtEnd)
                return _options.EndSentinel;
            var next = GetNextInternal(true);

            _flags &= ~SequenceStateTypes.StartOfInput;
            _stats.ItemsRead++;

            if (_index >= Length)
                _flags |= SequenceStateTypes.EndOfInput;

            if (next == '\n')
            {
                _line++;
                _column = 0;
                _flags |= SequenceStateTypes.StartOfLine;
                return next;
            }

            _flags &= ~SequenceStateTypes.StartOfLine;

            _column++;
            return next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char GetNextInternal(bool advance)
        {
            if (_index >= Length)
                return _options.EndSentinel;
            var value = _data[_index];
            if (advance)
                _index++;
            return value;
        }

        public char Peek()
        {
            var next = GetNextInternal(false);
            _stats.ItemsPeeked++;
            return next;
        }

        public Location CurrentLocation => new Location(_options.FileName, _line, _column);

        public bool IsAtEnd => _index >= Length;

        public SequenceStateTypes Flags => _flags;

        public int Consumed => _index;

        public int Index => _index;

        public char[] Data => _data;

        public int Length { get; }

        public string GetRemainder()
        {
            if (_index == 0)
                return new string(_data, 0, Length);
            if (_index >= Length)
                return string.Empty;
            return new string(_data, _index, Length - _index);
        }

        public void Reset()
        {
            _index = 0;
            _line = 1;
            _column = 0;
            _flags = FlagsForStartOfCharSequence(Length == 0);
        }

        public SequenceCheckpoint Checkpoint()
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(this, _index, _index, 0L, _flags, new Location(_options.FileName, _line, _column));
        }

        public void Rewind(SequenceCheckpoint checkpoint)
        {
            _stats.Rewinds++;
            _stats.RewindsToCurrentBuffer++;
            _index = checkpoint.Index;
            _line = checkpoint.Location.Line;
            _column = checkpoint.Location.Column;
            _flags = checkpoint.Flags;
        }

        public SequenceStatistics GetStatistics() => _stats.Snapshot();

        public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<char, TData, TResult> map)
        {
            NotNull(map);
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return map([], data);

            var size = end.Consumed - start.Consumed;
            var span = _data.AsSpan(start.Consumed, size);
            return map(span, data);
        }

        public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
            => GetBetween(start, end, (object?)null, static (b, _) => new string(b));

        public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
    }
}
