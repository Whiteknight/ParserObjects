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

        private bool _isComplete;
        private int _remainingBytes;
        private int _bufferIndex;
        private int _index;
        private int _consumed;

        public StreamByteSequence(string fileName, int bufferSize = 128)
        {
            Assert.ArgumentNotNullOrEmpty(fileName, nameof(fileName));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));

            _fileName = fileName;
            _bufferSize = bufferSize;
            _bufferIndex = bufferSize;
            _buffer = new byte[bufferSize];
            _stream = File.OpenRead(_fileName);
            _consumed = 0;
            FillBuffer();
        }

        public StreamByteSequence(Stream stream, string fileName = "", int bufferSize = 128)
        {
            Assert.ArgumentNotNull(stream, nameof(stream));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));

            _fileName = fileName;
            _bufferSize = bufferSize;
            _bufferIndex = bufferSize;
            _buffer = new byte[bufferSize];
            _stream = stream;
            _consumed = 0;
            FillBuffer();
        }

        public byte GetNext()
        {
            var b = GetNextByteRaw(true);
            if (b == 0)
                return b;
            _index++;
            _consumed++;
            return b;
        }

        public byte Peek() => GetNextByteRaw(false);

        public Location CurrentLocation => new Location(_fileName, 1, _index);

        public bool IsAtEnd => _isComplete;

        public int Consumed => _consumed;

        public void Dispose()
        {
            if (!_isComplete)
                _stream.Dispose();
        }

        private byte GetNextByteRaw(bool advance)
        {
            if (_isComplete)
                return 0;
            FillBuffer();
            if (_isComplete || _remainingBytes == 0 || _bufferIndex >= _bufferSize)
                return 0;
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
            {
                _isComplete = true;
                _stream.Dispose();
            }

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
            private readonly int _consumed;

            public SequenceCheckpoint(StreamByteSequence s, long currentPosition, int consumed)
            {
                _s = s;
                _currentPosition = currentPosition;
                _consumed = consumed;
            }

            public void Rewind() => _s.Rewind(_currentPosition, _consumed);
        }

        private void Rewind(long position, int consumed)
        {
            _stream.Seek(position, SeekOrigin.Begin);
            _remainingBytes = 0;
            _bufferIndex = _bufferSize;
            _isComplete = false;
            _consumed = consumed;
        }
    }
}
