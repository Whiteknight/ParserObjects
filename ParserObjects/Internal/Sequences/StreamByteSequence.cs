using System;
using System.Buffers;
using System.IO;
using static ParserObjects.Internal.Assert;

#pragma warning disable S1121

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence of bytes pulled from a Stream. Stream must support Read and Seek operations.
/// </summary>
public sealed class StreamByteSequence : ISequence<byte>, IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private readonly SequenceOptions<byte> _options;

    private WorkingSequenceStatistics _stats;
    private bool _isComplete;
    private int _remainingBytes;
    private int _bufferIndex;
    private int _consumed;

    public StreamByteSequence(SequenceOptions<byte> options)
    {
        NotNullOrEmpty(options.FileName);
        _options = options.Validate();

        _stats = default;
        _bufferIndex = _options.BufferSize;
        _buffer = new byte[_options.BufferSize];
        _stream = File.OpenRead(_options.FileName);
        _consumed = 0;
        Flags = SequenceStateTypes.StartOfInput;
        FillBuffer();
    }

    public StreamByteSequence(Stream stream, SequenceOptions<byte> options)
    {
        NotNull(stream);
        _options = options.Validate();

        _stats = default;
        _bufferIndex = _options.BufferSize;
        _buffer = new byte[_options.BufferSize];
        _stream = stream;
        if (!_stream.CanSeek || !_stream.CanRead)
            throw new InvalidOperationException("Stream must support Read and Seek to be used in a Sequence");
        _consumed = 0;
        Flags = SequenceStateTypes.StartOfInput;
        FillBuffer();
    }

    public byte GetNext()
    {
        var b = GetNextByteRaw(true);
        Flags = Flags.Without(SequenceStateTypes.StartOfInput);
        if (b == _options.EndSentinel)
            return b;
        _stats.ItemsRead++;
        _consumed++;
        return b;
    }

    public byte Peek()
    {
        var b = GetNextByteRaw(false);
        _stats.ItemsPeeked++;
        return b;
    }

    public Location CurrentLocation => new Location(_options.FileName, 1, _consumed);

    public bool IsAtEnd => _isComplete;

    public SequenceStateTypes Flags { get; private set; }

    public int Consumed => _consumed;

    public void Dispose()
    {
        _stream.Dispose();
    }

    private byte GetNextByteRaw(bool advance)
    {
        if (_isComplete || _remainingBytes == 0 || _bufferIndex >= _options.BufferSize)
            return _options.EndSentinel;

        var b = _buffer[_bufferIndex];

        if (advance)
        {
            _bufferIndex++;
            _remainingBytes--;
            if (_remainingBytes == 0)
                FillBuffer();
        }

        return b;
    }

    private void FillBuffer()
    {
        if (_remainingBytes != 0 && _bufferIndex < _options.BufferSize)
            return;
        if (_stream.Position >= _stream.Length)
        {
            _isComplete = true;
            Flags = Flags.With(SequenceStateTypes.EndOfInput);
            return;
        }

        _stats.BufferFills++;
        _remainingBytes = _stream.Read(_buffer, 0, _options.BufferSize);
        _bufferIndex = 0;
    }

    public SequenceCheckpoint Checkpoint()
    {
        // FillBuffer to make sure we have data and all our pointers are valid
        FillBuffer();
        _stats.CheckpointsCreated++;
        var currentPosition = _stream.Position - _remainingBytes;
        return new SequenceCheckpoint(this, _consumed, 0, currentPosition, Flags, new Location(_options.FileName, 1, _consumed));
    }

    public void Rewind(SequenceCheckpoint checkpoint)
    {
        _stats.Rewinds++;
        Flags = checkpoint.Flags;
        var position = checkpoint.StreamPosition;
        var consumed = checkpoint.Consumed;
        var sizeOfCurrentBuffer = _bufferIndex + _remainingBytes;
        var bufferStartPosition = _stream.Position - sizeOfCurrentBuffer;

        if (position >= bufferStartPosition && position < _stream.Position)
        {
            // The position is inside the current buffer, just update the pointers
            _stats.RewindsToCurrentBuffer++;
            _bufferIndex = (int)(position - bufferStartPosition);
            _remainingBytes = sizeOfCurrentBuffer - _bufferIndex;
            _isComplete = false;
            _consumed = consumed;
            return;
        }

        // The position is outside the current buffer, so we need to refill the buffer.
        // Try to fill the buffer such that the desired location is about 1/4th the way into the
        // buffer in case we need to make a few more quick rewinds in the vicinity.
        var bestStartPos = position - (_options.BufferSize >> 2);
        if (bestStartPos < 0)
            bestStartPos = 0;

        // Seek the stream to the desired start position and fill the buffer
        _stream.Seek(bestStartPos, SeekOrigin.Begin);
        var availableBytes = _stream.Read(_buffer, 0, _options.BufferSize);

        var forward = (int)(position - bestStartPos);
        _remainingBytes = availableBytes - forward;
        _bufferIndex = forward;
        _consumed = consumed;
        _isComplete = _remainingBytes <= 0;
    }

    public SequenceStatistics GetStatistics() => _stats.Snapshot();

    public TResult GetBetween<TData, TResult>(SequenceCheckpoint start, SequenceCheckpoint end, TData data, MapSequenceSpan<byte, TData, TResult> map)
    {
        NotNull(map);
        if (!Owns(start) || !Owns(end) || start.CompareTo(end) >= 0)
            return map([], data);

        var currentConsumed = Consumed;
        int startOfCurrentBuffer = currentConsumed - _bufferIndex;
        int endOfCurrentBuffer = Consumed + _remainingBytes;
        if (startOfCurrentBuffer <= start.Consumed && end.Consumed <= endOfCurrentBuffer)
        {
            // The requested range is entirely within the current buffer. So we can convert the
            // .Consumed values to indices and do a simple array copy.
            int startIndex = start.Consumed - startOfCurrentBuffer;
            int endIndex = end.Consumed - startOfCurrentBuffer;
            int localSize = endIndex - startIndex;
            Span<byte> span = _buffer.AsSpan(startIndex, localSize);
            return map(span, data);
        }

        // Otherwise the requested range is partially or wholly outside of the current buffer. So
        // we need to brute-force this by rewinding and calling .GetNext() in a loop
        var currentPosition = Checkpoint();
        start.Rewind();
        var size = end.Consumed - start.Consumed;

        byte[]? fromPool = null;
        Span<byte> buffer = size < 128
            ? stackalloc byte[size]
            : (fromPool = ArrayPool<byte>.Shared.Rent(size)).AsSpan(0, size);

        for (int i = 0; i < end.Consumed - start.Consumed; i++)
            buffer[i] = GetNext();

        currentPosition.Rewind();
        var result = map(buffer, data);
        if (fromPool != null)
            ArrayPool<byte>.Shared.Return(fromPool);
        return result;
    }

    public bool Owns(SequenceCheckpoint checkpoint) => checkpoint.Sequence == this;

    public void Reset()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _remainingBytes = _stream.Read(_buffer, 0, _options.BufferSize);
        if (_remainingBytes == 0)
            _isComplete = true;
        _consumed = 0;
        _bufferIndex = 0;
        Flags = SequenceStateTypes.StartOfInput;
        if (_isComplete)
            Flags = Flags.With(SequenceStateTypes.EndOfInput);
    }
}
