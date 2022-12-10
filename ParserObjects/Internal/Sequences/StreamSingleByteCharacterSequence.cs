using System;
using System.IO;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

public sealed class StreamSingleByteCharacterSequence : ISequence<char>, IDisposable
{
    private readonly StreamReader _reader;
    private readonly char[] _buffer;
    private readonly SequenceOptions<char> _options;

    private WorkingSequenceStatistics _stats;
    private int _totalCharsInBuffer;
    private int _line;
    private int _column;
    private int _index;
    private int _consumed;
    private long _bufferStartStreamPosition;

    public StreamSingleByteCharacterSequence(SequenceOptions<char> options)
    {
        Assert.ArgumentNotNullOrEmpty(options.FileName, nameof(options.FileName));
        _options = options;
        _options.Validate();
        if (!_options.Encoding!.IsSingleByte)
            throw new ArgumentException("This sequence is only for single-byte character encodings");

        _stats = default;
        _line = 0;
        _column = 0;
        _index = 0;
        _consumed = 0;
        _buffer = new char[_options.BufferSize];
        var stream = File.OpenRead(_options.FileName);
        _reader = new StreamReader(stream, _options.Encoding);
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _bufferStartStreamPosition = 0;
        _stats.BufferFills++;
    }

    public StreamSingleByteCharacterSequence(StreamReader reader, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(reader, nameof(reader));
        _options = options;
        _options.Encoding = reader.CurrentEncoding;
        _options.Validate();
        if (!_options.Encoding.IsSingleByte)
            throw new ArgumentException("This sequence is only for single-byte character encodings");

        _stats = default;
        _line = 0;
        _column = 0;
        _index = 0;
        _consumed = 0;
        _buffer = new char[_options.BufferSize];
        _reader = reader;
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _bufferStartStreamPosition = 0;
        _stats.BufferFills++;
    }

    public StreamSingleByteCharacterSequence(Stream stream, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(stream, nameof(stream));
        _options = options;
        _options.Validate();
        if (!_options.Encoding!.IsSingleByte)
            throw new ArgumentException("This sequence is only for single-byte character encodings");

        _stats = default;
        _line = 0;
        _column = 0;
        _index = 0;
        _consumed = 0;
        _buffer = new char[_options.BufferSize];
        _reader = new StreamReader(stream, _options.Encoding);
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _bufferStartStreamPosition = 0;
        _stats.BufferFills++;
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
        _consumed++;

        if (c == '\n')
        {
            _line++;
            _column = 0;
            return c;
        }

        _column++;
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

    public int Consumed => _consumed;

    public void Dispose()
    {
        _reader.Dispose();
    }

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
        FillBuffer();
        return c;
    }

    private void FillBuffer()
    {
        // If there are chars remaining in the current buffer, bail. There's nothing to do
        if (_index < _totalCharsInBuffer || _totalCharsInBuffer == 0)
            return;

        _stats.BufferFills++;
        _reader.DiscardBufferedData();
        _bufferStartStreamPosition = _reader.BaseStream.Position;
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _index = 0;
    }

    public SequenceCheckpoint Checkpoint()
    {
        _stats.CheckpointsCreated++;
        return new SequenceCheckpoint(this, _consumed, _index, _bufferStartStreamPosition, new Location(_options.FileName, _line, _column));
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _stats.Rewinds++;

        var bufferStartStreamPosition = checkpoint.StreamPosition;

        // If we're rewinding to the current position, short-circuit
        if (_bufferStartStreamPosition == bufferStartStreamPosition && _index == checkpoint.Index)
        {
            _stats.RewindsToCurrentBuffer++;
            return;
        }

        if (_bufferStartStreamPosition == bufferStartStreamPosition)
        {
            // We're rewinding to a place inside the current buffer
            _stats.RewindsToCurrentBuffer++;
            _index = checkpoint.Index;
            _line = checkpoint.Location.Line;
            _column = checkpoint.Location.Column;
            _consumed = checkpoint.Consumed;
            return;
        }

        // The position is outside the current buffer, so we need to refill the buffer.
        // Try to fill the buffer such that the desired location is about 1/4th the way into the
        // buffer in case we need to make a few more quick rewinds in the vicinity.
        var streamPosition = bufferStartStreamPosition + checkpoint.Index;
        var bestStartPos = streamPosition - (_options.BufferSize >> 2);
        if (bestStartPos < 0)
            bestStartPos = 0;

        // Seek the stream to the desired start position and fill the buffer
        _bufferStartStreamPosition = bestStartPos;
        _reader.BaseStream.Seek(bestStartPos, SeekOrigin.Begin);
        _reader.DiscardBufferedData();
        _totalCharsInBuffer = _reader.Read(_buffer, 0, _options.BufferSize);
        _consumed = checkpoint.Consumed;

        _index = (int)(streamPosition - bestStartPos);
        _line = checkpoint.Location.Line;
        _column = checkpoint.Location.Column;
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
