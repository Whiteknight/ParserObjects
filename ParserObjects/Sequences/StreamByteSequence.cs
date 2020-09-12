using System;
using System.Collections.Generic;
using System.IO;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    public sealed class StreamByteSequence : ISequence<byte>, IDisposable
    {
        private const int BufferSize = 128;

        private readonly string _fileName;
        private readonly Stream _stream;
        private readonly Stack<byte> _putbacks;
        private readonly byte[] _buffer;

        private bool _isComplete;
        private int _remainingBytes;
        private int _bufferIndex;
        private int _index;

        public StreamByteSequence(string fileName)
        {
            Assert.ArgumentNotNullOrEmpty(fileName, nameof(fileName));

            _fileName = fileName;
            _putbacks = new Stack<byte>();
            _bufferIndex = BufferSize;
            _buffer = new byte[BufferSize];
            _stream = File.OpenRead(_fileName);
        }

        public StreamByteSequence(Stream stream, string fileName = null)
        {
            Assert.ArgumentNotNull(stream, nameof(stream));
            _fileName = fileName;
            _putbacks = new Stack<byte>();
            _bufferIndex = BufferSize;
            _buffer = new byte[BufferSize];
            _stream = stream;
        }

        public byte GetNext()
        {
            var b = GetNextByteRaw(true);
            _index++;
            return b;
        }

        public void PutBack(byte value)
        {
            _putbacks.Push(value);
        }

        public byte Peek() => GetNextByteRaw(false);

        public Location CurrentLocation => new Location(_fileName, 1, _index);

        public bool IsAtEnd => _putbacks.Count == 0 && _isComplete;

        public void Dispose()
        {
            _stream?.Dispose();
        }

        private byte GetNextByteRaw(bool advance)
        {
            if (_putbacks.Count > 0)
                return advance ? _putbacks.Pop() : _putbacks.Peek();
            if (_isComplete)
                return 0;
            FillBuffer();
            if (_isComplete || _remainingBytes == 0 || _bufferIndex >= BufferSize)
                return 0;
            var b = _buffer[_bufferIndex];
            if (advance)
            {
                _bufferIndex++;
                _remainingBytes--;
            }
            return b;
        }

        private void FillBuffer()
        {
            if (_remainingBytes != 0 && _bufferIndex < BufferSize)
                return;
            _remainingBytes = _stream.Read(_buffer, 0, BufferSize);
            if (_remainingBytes == 0)
                _isComplete = true;
            _bufferIndex = 0;
        }
    }
}