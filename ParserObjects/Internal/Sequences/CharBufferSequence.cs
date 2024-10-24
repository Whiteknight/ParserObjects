using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using static ParserObjects.Internal.Sequences.SequenceFlags;

namespace ParserObjects.Internal.Sequences;

public static class CharBufferSequence
{
    private struct InternalState<TData>
    {
        private readonly Func<TData, int, char> _getCharAt;
        private readonly SequenceOptions<char> _options;

        private WorkingSequenceStatistics _stats;
        private int _index;
        private int _line;
        private int _column;

        public InternalState(SequenceOptions<char> options, TData data, int length, Func<TData, int, char> getCharAt)
        {
            _options = options;
            _options.Validate();
            Data = data;
            Length = length;
            _getCharAt = getCharAt;
            _stats = default;
            _index = 0;
            _line = 1;
            _column = 0;
            Flags = FlagsForStartOfCharSequence(Length == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetNext()
        {
            if (IsAtEnd)
                return _options.EndSentinel;
            var next = GetNextInternal(true);

            Flags = Flags.Without(SequencePositionFlags.StartOfInput);
            _stats.ItemsRead++;

            if (_index >= Length)
                Flags = Flags.With(SequencePositionFlags.EndOfInput);

            // If we have a newline, update the line-tracking.
            if (next == '\n')
            {
                _line++;
                _column = 0;
                Flags = Flags.With(SequencePositionFlags.StartOfLine);
                return next;
            }

            Flags = Flags.Without(SequencePositionFlags.StartOfLine);

            // Bump counts and return
            _column++;
            return next;
        }

        private char GetNextInternal(bool advance)
        {
            if (_index >= Length)
                return _options.EndSentinel;
            var value = _getCharAt(Data, _index);
            if (advance)
                _index++;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char Peek()
        {
            var next = GetNextInternal(false);
            _stats.ItemsPeeked++;
            return next;
        }

        public readonly Location CurrentLocation => new Location(_options.FileName, _line, _column);

        public readonly bool IsAtEnd => _index >= Length;

        public SequencePositionFlags Flags { get; private set; }

        public readonly int Index => _index;

        public TData Data { get; }

        public int Length { get; }

        public void Reset()
        {
            _index = 0;
            _line = 1;
            _column = 0;
            Flags = FlagsForStartOfCharSequence(Length == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceCheckpoint Checkpoint(ISequence sequence)
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(sequence, _index, _index, 0L, Flags, new Location(_options.FileName, _line, _column));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rewind(SequenceCheckpoint checkpoint)
        {
            _stats.Rewinds++;
            _stats.RewindsToCurrentBuffer++;
            _index = checkpoint.Index;
            _line = checkpoint.Location.Line;
            _column = checkpoint.Location.Column;
            Flags = checkpoint.Flags;
        }

        public readonly SequenceStatistics GetStatistics() => _stats.Snapshot();
    }

    /// <summary>
    /// A sequence of characters read from a string. Does not do any normalization of line endings.
    /// This type is an optimization for cases where we have a string and we explicitly do not need
    /// normalization. We can save a lot of up-front work in that case and store the string as-is.
    /// </summary>
    public sealed class FromNonnormalizedString : ICharSequence
    {
        private InternalState<string> _internal;

        public FromNonnormalizedString(string s, SequenceOptions<char> options)
        {
            Assert.ArgumentNotNull(s);
            Debug.Assert(options.MaintainLineEndings, "Only used when line-ending normalization is off");
            _internal = new InternalState<string>(options, s, s.Length, static (str, i) => str[i]);
        }

        public char GetNext() => _internal.GetNext();

        public char Peek() => _internal.Peek();

        public Location CurrentLocation => _internal.CurrentLocation;

        public bool IsAtEnd => _internal.IsAtEnd;

        public SequencePositionFlags Flags => _internal.Flags;

        public int Consumed => _internal.Index;

        public string GetRemainder()
        {
            if (_internal.Index == 0)
                return _internal.Data;
            if (_internal.Index >= _internal.Data.Length)
                return string.Empty;
            return _internal.Data.Substring(_internal.Index);
        }

        public void Reset() => _internal.Reset();

        public SequenceCheckpoint Checkpoint() => _internal.Checkpoint(this);

        public void Rewind(SequenceCheckpoint checkpoint) => _internal.Rewind(checkpoint);

        public SequenceStatistics GetStatistics() => _internal.GetStatistics();

        public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<char, TData, TResult> map)
        {
            Assert.ArgumentNotNull(map);
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return map(ReadOnlySpan<char>.Empty, data);

            var size = end.Consumed - start.Consumed;
            var span = _internal.Data.AsSpan(start.Consumed, size);
            return map(span, data);
        }

        public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
        {
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return string.Empty;

            int size = end.Consumed - start.Consumed;
            return _internal.Data.Substring(start.Consumed, size);
        }

        public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
    }

    /// <summary>
    /// A sequence of characters stored in an in-memory array. May or may not normalize line endings.
    /// </summary>
    public sealed class FromCharArray : ICharSequence
    {
        private InternalState<char[]> _internal;

        public FromCharArray(string s, SequenceOptions<char> options)
        {
            Assert.ArgumentNotNull(s);
            (var buffer, int bufferLength) = Normalize(s, options.NormalizeLineEndings);
            _internal = new InternalState<char[]>(options, buffer, bufferLength, static (d, i) => d[i]);
        }

        public FromCharArray(IReadOnlyList<char> s, SequenceOptions<char> options)
        {
            Assert.ArgumentNotNull(s);
            (var buffer, int bufferLength) = Normalize(s, options.NormalizeLineEndings);
            _internal = new InternalState<char[]>(options, buffer, bufferLength, static (d, i) => d[i]);
        }

        private static (char[] buffer, int length) Normalize(string s, bool normalize)
        {
            Debug.Assert(normalize, "Factory method should create a different class for non-normalized strings");

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

        private static (char[] buffer, int length) Normalize(IReadOnlyList<char> s, bool normalize)
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

        public char GetNext() => _internal.GetNext();

        public char Peek() => _internal.Peek();

        public Location CurrentLocation => _internal.CurrentLocation;

        public bool IsAtEnd => _internal.IsAtEnd;

        public SequencePositionFlags Flags => _internal.Flags;

        public int Consumed => _internal.Index;

        public string GetRemainder()
        {
            if (_internal.Index == 0)
                return new string(_internal.Data, 0, _internal.Length);
            if (_internal.Index >= _internal.Length)
                return string.Empty;
            return new string(_internal.Data, _internal.Index, _internal.Length - _internal.Index);
        }

        public void Reset() => _internal.Reset();

        public SequenceCheckpoint Checkpoint() => _internal.Checkpoint(this);

        public void Rewind(SequenceCheckpoint checkpoint) => _internal.Rewind(checkpoint);

        public SequenceStatistics GetStatistics() => _internal.GetStatistics();

        public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<char, TData, TResult> map)
        {
            Assert.ArgumentNotNull(map);
            if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
                return map(ReadOnlySpan<char>.Empty, data);

            var size = end.Consumed - start.Consumed;
            var span = _internal.Data.AsSpan(start.Consumed, size);
            return map(span, data);
        }

        public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
            => GetBetween(start, end, (object?)null, static (b, _) => new string(b));

        public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
    }
}
