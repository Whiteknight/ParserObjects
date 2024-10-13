using System;
using System.Buffers;
using System.IO;

namespace ParserObjects.Internal.Sequences;

public sealed class StreamSingleByteCharacterSequence : ICharSequence, IDisposable
{
    private readonly byte[] _byteBuffer;
    private readonly char[] _buffer;
    private readonly SequenceOptions<char> _options;
    private readonly Stream _stream;

    private WorkingSequenceStatistics _stats;
    private int _totalCharsInBuffer;
    private int _line;
    private int _column;
    private int _index;
    private int _consumed;
    private long _bufferStartStreamPosition;

    public StreamSingleByteCharacterSequence(Stream stream, SequenceOptions<char> options)
    {
        Assert.ArgumentNotNull(stream);
        _options = options;
        _options.Validate();
        if (!_options.Encoding!.IsSingleByte)
            throw new ArgumentException("This sequence is only for single-byte character encodings");

        _stats = default;
        _line = 0;
        _column = 0;
        _index = 0;
        _consumed = 0;
        _byteBuffer = new byte[_options.BufferSize];
        _buffer = new char[_options.BufferSize];
        _stream = stream;
        _totalCharsInBuffer = ReadStream();
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
        _stream.Dispose();
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
        _bufferStartStreamPosition = _stream.Position;
        ReadStream();
        _index = 0;
    }

    private int ReadStream()
    {
        _totalCharsInBuffer = _stream.Read(_byteBuffer, 0, _options.BufferSize);
        if (_totalCharsInBuffer > 0)
            _options.Encoding!.GetChars(_byteBuffer, 0, _totalCharsInBuffer, _buffer, 0);
        return _totalCharsInBuffer;
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
        _stream.Seek(bestStartPos, SeekOrigin.Begin);
        ReadStream();
        _consumed = checkpoint.Consumed;

        _index = (int)(streamPosition - bestStartPos);
        _line = checkpoint.Location.Line;
        _column = checkpoint.Location.Column;
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<char, TData, TResult> map)
    {
        Assert.ArgumentNotNull(map);
        if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
            return map(ReadOnlySpan<char>.Empty, data);

        var currentPosition = Checkpoint();
        start.Rewind();
        var size = end.Consumed - start.Consumed;

        char[]? fromPool = null;
        Span<char> buffer = size < 128
            ? stackalloc char[size]
            : (fromPool = ArrayPool<char>.Shared.Rent(size)).AsSpan(0, size);

        for (int i = 0; i < end.Consumed - start.Consumed; i++)
            buffer[i] = GetNext();

        currentPosition.Rewind();
        var result = map(buffer, data);
        if (fromPool != null)
            ArrayPool<char>.Shared.Return(fromPool);
        return result;
    }

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;

    public string GetRemainder()
    {
        var cp = Checkpoint();

        _stream.Seek(_bufferStartStreamPosition + _index, SeekOrigin.Begin);
        var reader = new StreamReader(_stream, _options.Encoding!);
        var s = reader.ReadToEnd();

        cp.Rewind();

        return s;
    }

    public void Reset()
    {
        _bufferStartStreamPosition = 0;
        _stream.Seek(0, SeekOrigin.Begin);
        ReadStream();
        _index = 0;
        _line = 0;
        _column = 0;
        _consumed = 0;
    }
}
