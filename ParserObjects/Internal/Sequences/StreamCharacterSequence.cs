using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using static ParserObjects.Internal.Assert;
using static ParserObjects.Internal.Sequences.SequenceFlags;

#pragma warning disable S1121

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence of characters read from a Stream or StreamReader, such as from a file.
/// </summary>
public sealed class StreamCharacterSequence : ICharSequence, IDisposable
{
    private readonly char[] _surrogateBuffer = new char[2];
    private readonly char[] _tempBuffer = new char[1];
    private readonly StreamReader _reader;
    private readonly char[] _buffer;
    private readonly SequenceOptions<char> _options;
    private readonly long[] _bufferStartPositions;

    private WorkingSequenceStatistics _stats;
    private int _totalCharsInBuffer;
    private int _line;
    private int _column;
    private int _index;
    private long _streamPosition;

    public StreamCharacterSequence(StreamReader reader, SequenceOptions<char> options)
    {
        _options = options.Validate() with { Encoding = reader.CurrentEncoding };
        _stats = default;
        _buffer = new char[_options.BufferSize];
        _reader = NotNull(reader);
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _stats.BufferFills++;
        _bufferStartPositions = new long[(_reader.BaseStream.Length / _options.BufferSize) + 1];
        _bufferStartPositions[0] = 0L;
        Flags = FlagsForStartOfCharSequence(IsAtEnd);
    }

    public StreamCharacterSequence(Stream stream, SequenceOptions<char> options)
    {
        NotNull(stream);
        _options = options.Validate();
        _stats = default;
        _buffer = new char[_options.BufferSize];
        _reader = new StreamReader(stream, _options.Encoding!);
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _stats.BufferFills++;
        _bufferStartPositions = new long[(_reader.BaseStream.Length / _options.BufferSize) + 1];
        _bufferStartPositions[0] = 0L;
        Flags = FlagsForStartOfCharSequence(IsAtEnd);
    }

    /*
     * For any buffer we need the char[] array of characters in the buffer and the long
     * position in the stream where that buffer starts.
     *
     * For a next buffer, the position where the buffer starts is the same as the position
     * where the previous buffer ended (position of the start of the last char + the width of
     * that char).
     *
     * For any rewind buffer, we can get the position from the checkpoint and use that to fill
     * the buffer by resetting the stream to that position and clearing the reader buffer.
     */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEncodingByteCountForCharacter(char c)
    {
        _tempBuffer[0] = c;
        return _options.Encoding!.GetByteCount(_tempBuffer.AsSpan());
    }

    public char GetNext()
    {
        if (IsAtEnd)
            return _options.EndSentinel;

        var c = GetNextCharRaw(true);
        Flags = Flags.Without(SequenceStateTypes.StartOfInput);

        if (_options.NormalizeLineEndings && c == '\r')
        {
            if (GetNextCharRaw(false) == '\n')
                GetNextCharRaw(true);
            c = '\n';
        }

        _stats.ItemsRead++;
        if (IsAtEnd)
            Flags = Flags.With(SequenceStateTypes.EndOfInput);

        if (c == '\n')
        {
            _line++;
            _column = 0;
            Flags = Flags.With(SequenceStateTypes.StartOfLine);
            return c;
        }

        _column++;
        Flags = Flags.Without(SequenceStateTypes.StartOfLine);
        return c;
    }

    public char Peek()
    {
        var next = GetNextCharRaw(false);
        if (_options.NormalizeLineEndings && next == '\r')
            next = '\n';
        _stats.ItemsPeeked++;
        return next;
    }

    public Location CurrentLocation => new Location(_options.FileName, _line + 1, _column);

    public bool IsAtEnd => _totalCharsInBuffer == 0;

    public SequenceStateTypes Flags { get; private set; }

    public int Consumed { get; private set; }

    public void Dispose()
    {
        _reader.Dispose();
    }

    private bool _expectLowSurrogate;

    // This one is hard, if not impossible to test because the StreamReader isn't going to put
    // unmatched high surrogates into the char buffer. We're going to keep a lot of this
    // if (error) throw... logic just to be safe, but expect it to not come up much.
    [ExcludeFromCodeCoverage]
    private char GetLowSurrogateOrThrow()
    {
        if (_index < _totalCharsInBuffer)
        {
            var candidate = _buffer[_index];
            if (!char.IsLowSurrogate(candidate))
                throw new InvalidOperationException("Found high surrogate but could not find corresponding low surrogate. Input is malformed.");
            return candidate;
        }

        FillBuffer();
        if (_totalCharsInBuffer == 0)
            throw new InvalidOperationException("Found high surrogate but there were no more characters. Input is malformed.");
        var next = _buffer[_index];
        if (!char.IsLowSurrogate(next))
            throw new InvalidOperationException("Found high surrogate but could not find corresponding low surrogate. Input is malformed.");
        return next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char GetNextCharRaw(bool advance)
    {
        // We fill the buffer initially in the constructor, and then again after we get a
        // character. If the buffer doesn't have any data in it at this point, it's because
        // we're at the end of input.
        if (_totalCharsInBuffer == 0)
            return _options.EndSentinel;

        var c = _buffer[_index];
        if (!advance)
            return c;

        // Bump the index so we advance forward
        _index++;
        Consumed++;

        if (_expectLowSurrogate)
        {
            // We've seen the low surrogate for the size calculation. Assert, clear the flag.
            // Don't advance the stream position because we already did that when we saw the
            // high surrogate
            Debug.Assert(char.IsLowSurrogate(c));
            _expectLowSurrogate = false;
            FillBuffer();
            return c;
        }

        if (char.IsHighSurrogate(c))
        {
            // If c is a high surrogate we MUST get next char to find the matching low surrogate.
            // That is the only way to get a valid size calculation for stream position. Set
            // a flag to say that we are in between surrogates so we don't do something stupid
            // like try to set a checkpoint.
            var low = GetLowSurrogateOrThrow();
            _surrogateBuffer[0] = c;
            _surrogateBuffer[1] = low;
            var totalSize = _options.Encoding!.GetByteCount(_surrogateBuffer);
            _streamPosition += totalSize;
            _expectLowSurrogate = true;
            FillBuffer();
            return c;
        }

        var size = GetEncodingByteCountForCharacter(c);
        _streamPosition += size;
        FillBuffer();
        return c;
    }

    // Called during normal read operations to fill the next buffer when we've read to the end of
    // the current buffer.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillBuffer()
    {
        // If there are chars remaining in the current buffer, bail. There's nothing to do
        if (_index < _totalCharsInBuffer || _totalCharsInBuffer == 0)
            return;

        /* There is a real problem here where when we get to end-of-stream (EOS) the
         * _totalCharsInBuffer is 0 so we try to refill, but we're at the end of the stream
         * so there's nothing to get and Consumed is not increasing. Without some protection in
         * place it would happily overwrite the start position for the last buffer.
         */
        int buffersStartIndex = Consumed / _options.BufferSize;
        if (buffersStartIndex != 0 && _bufferStartPositions[buffersStartIndex] == 0)
            _bufferStartPositions[buffersStartIndex] = _streamPosition;

        _stats.BufferFills++;
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _index = 0;
    }

    public SequenceCheckpoint Checkpoint()
    {
        if (_expectLowSurrogate)
            throw new InvalidOperationException("Cannot set a checkpoint between the high and low surrogates of a single codepoint");
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, Consumed, _index, _streamPosition, Flags, new Location(_options.FileName, _line, _column));
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        // Clear this flag, just in case
        _expectLowSurrogate = false;
        Flags = checkpoint.Flags;

        _stats.Rewinds++;

        var streamPosition = checkpoint.StreamPosition;
        int buffersStartIndex = checkpoint.Consumed / _options.BufferSize;
        var bufferStartPosition = _bufferStartPositions[buffersStartIndex];

        // If we're rewinding to the current position, short-circuit
        if (_streamPosition == streamPosition)
        {
            _stats.RewindsToCurrentBuffer++;
            return;
        }

        var currentBuffersStartIndex = Consumed / _options.BufferSize;
        var currentBufferStartPosition = _bufferStartPositions[currentBuffersStartIndex];

        _streamPosition = streamPosition;
        _line = checkpoint.Location.Line;
        _column = checkpoint.Location.Column;
        Consumed = checkpoint.Consumed;
        _index = checkpoint.Index;

        if (currentBufferStartPosition == bufferStartPosition)
        {
            // Rewinding to the current buffer
            _stats.RewindsToCurrentBuffer++;
            return;
        }

        // Otherwise we reset the buffer starting at bufferStartStreamPosition
        _reader.BaseStream.Seek(bufferStartPosition, SeekOrigin.Begin);
        _reader.DiscardBufferedData();
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<char, TData, TResult> map)
    {
        NotNull(map);
        if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
            return map([], data);

        var currentPosition = Checkpoint();
        start.Rewind();
        var size = end.Consumed - start.Consumed;

        char[]? fromPool = null;
        Span<char> buffer = size < 128
            ? stackalloc char[size]
            : (fromPool = ArrayPool<char>.Shared.Rent(size)).AsSpan(0, size);

        for (int i = 0; i < size; i++)
            buffer[i] = GetNext();

        currentPosition.Rewind();
        var result = map(buffer, data);
        if (fromPool != null)
            ArrayPool<char>.Shared.Return(fromPool);
        return result;
    }

    public string GetStringBetween(SequenceCheckpoint start, SequenceCheckpoint end)
        => GetBetween(start, end, (object?)null, static (b, _) => new string(b));

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;

    public string GetRemainder()
    {
        if (_expectLowSurrogate)
            throw new InvalidOperationException("Cannot get remainder from inside a multi-char codepoint");

        var cp = Checkpoint();

        _reader.BaseStream.Seek(_streamPosition, SeekOrigin.Begin);
        _reader.DiscardBufferedData();

        var s = _reader.ReadToEnd();

        cp.Rewind();

        return s;
    }

    public void Reset()
    {
        // Attempt to reset the stream and reader back to the beginning
        _reader.BaseStream.Seek(0, SeekOrigin.Begin);
        _reader.DiscardBufferedData();
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _index = 0;
        _line = 0;
        _column = 0;
        _streamPosition = 0;
        _expectLowSurrogate = false;
        Consumed = 0;
        _stats.BufferFills++;
        Flags = FlagsForStartOfCharSequence(IsAtEnd);
    }
}
