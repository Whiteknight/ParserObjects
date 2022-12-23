using System;
using System.Diagnostics;
using System.IO;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence of characters read from a Stream or StreamReader, such as from a file.
/// </summary>
public sealed class StreamCharacterSequence : ISequence<char>, IDisposable
{
    private static readonly char[] _surrogateBuffer = new char[2];

    private readonly StreamReader _reader;
    private readonly char[] _buffer;
    private readonly SequenceOptions<char> _options;

    private WorkingSequenceStatistics _stats;
    private BufferMetadata _metadata;

    // TODO: I don't think we need BufferMetadata struct anymore. We can just put these fields
    // directly in the class, because we don't copy the entire struct anymore for checkpointing.
    private struct BufferMetadata
    {
        public int TotalCharsInBuffer { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int Index { get; set; }
        public int Consumed { get; set; }
        public long StreamPosition { get; set; }
        public long BufferStartStreamPosition { get; set; }
    }

    public StreamCharacterSequence(StreamReader reader, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(reader, nameof(reader));
        _options = options;
        _options.Encoding = reader.CurrentEncoding;
        _options.Validate();
        _stats = default;
        _metadata = default;
        _buffer = new char[_options.BufferSize];
        _reader = reader;
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _metadata.BufferStartStreamPosition = 0;
        _stats.BufferFills++;
    }

    public StreamCharacterSequence(Stream stream, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(stream, nameof(stream));
        _options = options;
        _options.Validate();
        _stats = default;
        _metadata = default;
        _buffer = new char[_options.BufferSize];
        _reader = new StreamReader(stream, _options.Encoding!);
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _metadata.BufferStartStreamPosition = 0;
        _stats.BufferFills++;
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

    private int GetEncodingByteCountForCharacter(char c)
    {
        unsafe
        {
            return _options.Encoding!.GetByteCount(&c, 1);
        }
    }

    public char GetNext()
    {
        var c = GetNextCharRaw(true);
        if (c == _options.EndSentinel)
            return c;

        if (_options.NormalizeLineEndings && c == '\r')
        {
            if (GetNextCharRaw(false) == '\n')
                GetNextCharRaw(true);
            c = '\n';
        }

        _stats.ItemsRead++;

        if (c == '\n')
        {
            _metadata.Line++;
            _metadata.Column = 0;
            return c;
        }

        _metadata.Column++;
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

    public Location CurrentLocation => new Location(_options.FileName, _metadata.Line + 1, _metadata.Column);

    public bool IsAtEnd => _metadata.TotalCharsInBuffer == 0;

    public int Consumed => _metadata.Consumed;

    public void Dispose()
    {
        _reader.Dispose();
    }

    private bool _expectLowSurrogate;

    private char GetLowSurrogateOrThrow()
    {
        if (_metadata.Index < _metadata.TotalCharsInBuffer)
        {
            var candidate = _buffer[_metadata.Index];
            if (!char.IsLowSurrogate(candidate))
                throw new InvalidOperationException("Found high surrogate but could not find corresponding low surrogate. Input is malformed.");
            return candidate;
        }

        FillBuffer();
        if (_metadata.TotalCharsInBuffer == 0)
            throw new InvalidOperationException("Found high surrogate but there were no more characters. Input is malformed.");
        var next = _buffer[_metadata.Index];
        if (!char.IsLowSurrogate(next))
            throw new InvalidOperationException("Found high surrogate but could not find corresponding low surrogate. Input is malformed.");
        return next;
    }

    private char GetNextCharRaw(bool advance)
    {
        // We fill the buffer initially in the constructor, and then again after we get a
        // character. If the buffer doesn't have any data in it at this point, it's because
        // we're at the end of input.
        if (_metadata.TotalCharsInBuffer == 0)
            return _options.EndSentinel;

        var c = _buffer[_metadata.Index];
        if (!advance)
            return c;

        // Bump the index so we advance forward
        _metadata.Index++;
        _metadata.Consumed++;

        if (_expectLowSurrogate)
        {
            // We've seen the low surrogate for the size calculation. Assert, clear the flag.
            // Don't advance the stream position because we already did that when we saw the
            // high surrogate
            Debug.Assert(char.IsLowSurrogate(c), "Make sure the low surrogate is still there");
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
            _metadata.StreamPosition += totalSize;
            _expectLowSurrogate = true;
            FillBuffer();
            return c;
        }

        var size = GetEncodingByteCountForCharacter(c);
        _metadata.StreamPosition += size;
        FillBuffer();
        return c;
    }

    private void FillBuffer()
    {
        // If there are chars remaining in the current buffer, bail. There's nothing to do
        if (_metadata.Index < _metadata.TotalCharsInBuffer || _metadata.TotalCharsInBuffer == 0)
            return;

        _stats.BufferFills++;
        _metadata.BufferStartStreamPosition = _metadata.StreamPosition;
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _metadata.Index = 0;
    }

    public SequenceCheckpoint Checkpoint()
    {
        if (_expectLowSurrogate)
            throw new InvalidOperationException("Cannot set a checkpoint between the high and low surrogates of a single codepoint");
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _metadata.Consumed, _metadata.Index, _metadata.StreamPosition, new Location(_options.FileName, _metadata.Line, _metadata.Column));
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        // Clear this flag, just in case
        _expectLowSurrogate = false;

        _stats.Rewinds++;

        var streamPosition = checkpoint.StreamPosition;

        // If we're rewinding to the current position, short-circuit
        if (_metadata.StreamPosition == streamPosition)
        {
            _stats.RewindsToCurrentBuffer++;
            return;
        }

        // TODO: It would be nice to be able to rewind to the current buffer. But we would need
        // three pieces of information: The BufferStartStreamPosition to line us up to the correct
        // buffer, the character Index in the buffer, and the Stream Position associated with that
        // index, so we can resume the count. This is a lot of extra data to add to SequenceCheckpoint
        // that many other sequences won't use.

        // Otherwise we reset the buffer starting at bufferStartStreamPosition
        _metadata.BufferStartStreamPosition = streamPosition;
        _metadata.StreamPosition = streamPosition;
        _metadata.Index = 0;
        _metadata.Line = checkpoint.Location.Line;
        _metadata.Column = checkpoint.Location.Column;
        _metadata.Consumed = checkpoint.Consumed;
        _reader.BaseStream.Seek(_metadata.BufferStartStreamPosition, SeekOrigin.Begin);
        _reader.DiscardBufferedData();
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public char[] GetBetween(SequenceCheckpoint start, SequenceCheckpoint end)
    {
        if (!Owns(start) || !Owns(end))
            return Array.Empty<char>();

        if (start.CompareTo(end) >= 0)
            return Array.Empty<char>();

        var currentPosition = Checkpoint();
        start.Rewind();
        var buffer = new char[end.Consumed - start.Consumed];
        for (int i = 0; i < end.Consumed - start.Consumed; i++)
            buffer[i] = GetNext();
        currentPosition.Rewind();
        return buffer;
    }

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;
}
