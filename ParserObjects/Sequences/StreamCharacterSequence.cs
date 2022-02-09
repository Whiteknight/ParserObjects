using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ParserObjects.Utility;

namespace ParserObjects.Sequences;

/// <summary>
/// A sequence of characters read from a Stream or StreamReader, such as from a file.
/// </summary>
public sealed class StreamCharacterSequence : ISequence<char>, IDisposable
{
    private static readonly char[] _surrogateBuffer = new char[2];

    private readonly StreamReader _reader;
    private readonly Encoding _encoding;
    private readonly char[] _buffer;
    private readonly Options _options;

    private SequenceStatistics _stats;
    private BufferMetadata _metadata;

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

    public record struct Options(string FileName = "", int BufferSize = 1024, bool NormalizeLineEndings = true, char EndSentinel = '\0')
    {
        public void Validate()
        {
            if (FileName == null)
                FileName = "";
            if (BufferSize <= 0)
                BufferSize = 1024;
        }
    }

    public StreamCharacterSequence(Options options, Encoding? encoding = null)
    {
        Assert.ArgumentNotNullOrEmpty(options.FileName, nameof(options.FileName));
        options.Validate();
        _options = options;
        _stats = default;
        _metadata = default;
        _buffer = new char[_options.BufferSize];
        var stream = File.OpenRead(_options.FileName);
        _encoding = encoding ?? Encoding.UTF8;
        _reader = new StreamReader(stream, _encoding);
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
    }

    public StreamCharacterSequence(StreamReader reader, Options options)
    {
        Assert.ArgumentNotNull(reader, nameof(reader));
        options.Validate();
        _options = options;
        _stats = default;
        _metadata = default;
        _buffer = new char[_options.BufferSize];
        _reader = reader;
        _encoding = _reader.CurrentEncoding;
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
    }

    public StreamCharacterSequence(Stream stream, Options options, Encoding? encoding = null)
    {
        Assert.ArgumentNotNull(stream, nameof(stream));
        options.Validate();
        _options = options;
        _stats = default;
        _metadata = default;
        _buffer = new char[_options.BufferSize];
        _encoding = encoding ?? Encoding.UTF8;
        _reader = new StreamReader(stream, _encoding);
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
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
            return _encoding.GetByteCount(&c, 1);
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
            var totalSize = _encoding.GetByteCount(_surrogateBuffer);
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

    public ISequenceCheckpoint Checkpoint()
    {
        if (_expectLowSurrogate)
            throw new InvalidOperationException("Cannot set a checkpoint between the high and low surrogates of a single codepoint");
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _metadata);
    }

    private record SequenceCheckpoint(StreamCharacterSequence S, BufferMetadata Metadata)
        : ISequenceCheckpoint
    {
        public int Consumed => Metadata.Consumed;

        public Location Location => new Location(S._options.FileName, Metadata.Line + 1, Metadata.Column);

        public void Rewind() => S.Rewind(Metadata);
    }

    private void Rewind(BufferMetadata metadata)
    {
        // Clear this flag, just in case
        _expectLowSurrogate = false;

        _stats.Rewinds++;

        // If we're rewinding to the current position, short-circuit
        if (_metadata.BufferStartStreamPosition == metadata.BufferStartStreamPosition && _metadata.StreamPosition == metadata.StreamPosition)
        {
            _stats.RewindsToCurrentBuffer++;
            return;
        }

        // If position is within the current buffer, just reset _bufferIndex and the metadata
        // and continue
        if (_metadata.BufferStartStreamPosition == metadata.BufferStartStreamPosition)
        {
            _stats.RewindsToCurrentBuffer++;
            _metadata = metadata;
            return;
        }

        // Otherwise we reset the buffer starting at bufferStartStreamPosition
        _metadata = metadata;
        _reader.BaseStream.Seek(_metadata.BufferStartStreamPosition, SeekOrigin.Begin);
        _reader.DiscardBufferedData();
        _metadata.TotalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
    }

    public ISequenceStatistics GetStatistics() => _stats;
}
