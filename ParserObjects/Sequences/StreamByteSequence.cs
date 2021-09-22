using System;
using System.IO;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of bytes pulled from a Stream or StreamReader.
    /// </summary>
    public sealed class StreamByteSequence : ISequence<byte>, IDisposable
    {
        private readonly string _fileName;
        private readonly int _bufferSize;
        private readonly Stream _stream;
        private readonly byte[] _buffer;
        private readonly byte _endSentinel;

        private bool _isComplete;
        private int _remainingBytes;
        private int _bufferIndex;
        private int _consumed;

        public StreamByteSequence(string fileName, int bufferSize = 1024, byte endSentinel = 0)
        {
            Assert.ArgumentNotNullOrEmpty(fileName, nameof(fileName));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));

            _fileName = fileName;
            _bufferSize = bufferSize;
            _bufferIndex = bufferSize;
            _buffer = new byte[bufferSize];
            _stream = File.OpenRead(_fileName);
            _consumed = 0;
            _endSentinel = endSentinel;
            FillBuffer();
        }

        public StreamByteSequence(Stream stream, string fileName = "", int bufferSize = 1024, byte endSentinel = 0)
        {
            Assert.ArgumentNotNull(stream, nameof(stream));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));

            _fileName = fileName;
            _bufferSize = bufferSize;
            _bufferIndex = bufferSize;
            _buffer = new byte[bufferSize];
            _stream = stream;
            if (!_stream.CanSeek || !_stream.CanRead)
                throw new InvalidOperationException("Stream must support Read and Seek to be used in a Sequence");
            _consumed = 0;
            _endSentinel = endSentinel;
            FillBuffer();
        }

        public byte GetNext()
        {
            var b = GetNextByteRaw(true);
            if (b == _endSentinel)
                return b;
            _consumed++;
            return b;
        }

        public byte Peek() => GetNextByteRaw(false);

        public Location CurrentLocation => new Location(_fileName, 1, _consumed);

        public bool IsAtEnd => _isComplete;

        public int Consumed => _consumed;

        public void Dispose()
        {
            _stream.Dispose();
        }

        private byte GetNextByteRaw(bool advance)
        {
            if (_isComplete)
                return _endSentinel;
            if (_isComplete || _remainingBytes == 0 || _bufferIndex >= _bufferSize)
                return _endSentinel;
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
            if (_remainingBytes != 0 && _bufferIndex < _bufferSize)
                return;
            _remainingBytes = _stream.Read(_buffer, 0, _bufferSize);
            if (_remainingBytes == 0)
                _isComplete = true;
            _bufferIndex = 0;
        }

        public ISequenceCheckpoint Checkpoint()
        {
            // FillBuffer to make sure we have data and all our pointers are valid
            FillBuffer();
            var currentPosition = _stream.Position - _remainingBytes;
            return new SequenceCheckpoint(this, currentPosition, _consumed);
        }

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly StreamByteSequence _s;
            private readonly long _currentPosition;

            public SequenceCheckpoint(StreamByteSequence s, long currentPosition, int consumed)
            {
                _s = s;
                _currentPosition = currentPosition;
                Consumed = consumed;
            }

            public int Consumed { get; }

            public Location Location => new Location(_s._fileName, 1, Consumed);

            public void Rewind() => _s.Rewind(_currentPosition, Consumed);
        }

        private void Rewind(long position, int consumed)
        {
            var sizeOfCurrentBuffer = _bufferIndex + _remainingBytes;
            var bufferStartPosition = _stream.Position - sizeOfCurrentBuffer;

            if (position >= bufferStartPosition && position < _stream.Position)
            {
                // The position is inside the current buffer, just update the pointers
                _bufferIndex = (int)(position - bufferStartPosition);
                _remainingBytes = sizeOfCurrentBuffer - _bufferIndex;
                _isComplete = false;
                _consumed = consumed;
                return;
            }

            // The position is outside the current buffer, so we need to refill the buffer.

            var bestStartPos = position - (_bufferSize >> 2);
            if (bestStartPos < 0)
                bestStartPos = 0;
            var forward = (int)(position - bestStartPos);
            _stream.Seek(bestStartPos, SeekOrigin.Begin);
            var availableBytes = _stream.Read(_buffer, 0, _bufferSize);
            _remainingBytes = availableBytes - forward;
            _bufferIndex = forward;
            _consumed = consumed;
            _isComplete = _remainingBytes <= 0;
        }
    }
}
