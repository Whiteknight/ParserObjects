﻿using System;
using System.IO;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// A sequence of bytes pulled from a Stream. Stream must support Read and Seek operations.
/// </summary>
public sealed class StreamByteSequence : ISequence<byte>, IDisposable
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private readonly Options _options;

    private SequenceStatistics _stats;
    private bool _isComplete;
    private int _remainingBytes;
    private int _bufferIndex;
    private int _consumed;

    public record struct Options(string FileName, int BufferSize, byte EndSentinel)
    {
        public void Validate()
        {
            if (FileName == null)
                FileName = string.Empty;
            if (BufferSize <= 0)
                BufferSize = 1024;
        }
    }

    public StreamByteSequence(Options options)
    {
        options.Validate();
        _options = options;
        Assert.ArgumentNotNullOrEmpty(_options.FileName, nameof(_options.FileName));

        _stats = default;
        _bufferIndex = _options.BufferSize;
        _buffer = new byte[_options.BufferSize];
        _stream = File.OpenRead(_options.FileName);
        _consumed = 0;
        FillBuffer();
    }

    public StreamByteSequence(Stream stream, Options options)
    {
        Assert.ArgumentNotNull(stream, nameof(stream));

        options.Validate();
        _options = options;

        _stats = default;
        _bufferIndex = _options.BufferSize;
        _buffer = new byte[_options.BufferSize];
        _stream = stream;
        if (!_stream.CanSeek || !_stream.CanRead)
            throw new InvalidOperationException("Stream must support Read and Seek to be used in a Sequence");
        _consumed = 0;
        FillBuffer();
    }

    public byte GetNext()
    {
        var b = GetNextByteRaw(true);
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

    public int Consumed => _consumed;

    public void Dispose()
    {
        _stream.Dispose();
    }

    private byte GetNextByteRaw(bool advance)
    {
        if (_isComplete)
            return _options.EndSentinel;

        if (_remainingBytes == 0 || _bufferIndex >= _options.BufferSize)
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
        _stats.BufferFills++;
        _remainingBytes = _stream.Read(_buffer, 0, _options.BufferSize);
        if (_remainingBytes == 0)
            _isComplete = true;
        _bufferIndex = 0;
    }

    public ISequenceCheckpoint Checkpoint()
    {
        // FillBuffer to make sure we have data and all our pointers are valid
        FillBuffer();
        _stats.CheckpointsCreated++;
        var currentPosition = _stream.Position - _remainingBytes;
        return new SequenceCheckpoint(this, currentPosition, _consumed);
    }

    private record SequenceCheckpoint(StreamByteSequence S, long CurrentPosition, int Consumed)
        : ISequenceCheckpoint
    {
        public Location Location => new Location(S._options.FileName, 1, Consumed);

        public void Rewind() => S.Rewind(CurrentPosition, Consumed);
    }

    private void Rewind(long position, int consumed)
    {
        _stats.Rewinds++;
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

    public ISequenceStatistics GetStatistics() => _stats;
}