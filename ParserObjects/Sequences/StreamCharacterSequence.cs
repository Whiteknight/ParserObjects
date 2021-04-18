using System;
using System.IO;
using System.Text;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of characters read from a Stream, such as from a file.
    /// </summary>
    public sealed class StreamCharacterSequence : ISequence<char>, IDisposable
    {
        private readonly int _bufferSize;
        private readonly string _fileName;
        private readonly StreamReader _reader;
        private readonly bool _normalizeLineEndings;
        private readonly char _endSentinel;

        // Linked list of buffer nodes
        private BufferNode _currentBuffer;

        // True if we're at the end of the stream and the stream is disposed. False otherwise.
        private bool _isComplete;

        // Location in the current buffer of the next char to read.
        private int _bufferIndex;

        // Current location information
        private int _line;

        private int _column;

        private class BufferNode
        {
            public BufferNode(int bufferSize, StreamReader reader, int startConsumed)
            {
                Buffer = new char[bufferSize];
                TotalChars = reader.Read(Buffer, 0, bufferSize);
                StartConsumed = startConsumed;
                Next = null;
            }

            public char[] Buffer { get; }
            public BufferNode? Next { get; set; }
            public int TotalChars { get; }
            public int StartConsumed { get; }
        }

        public StreamCharacterSequence(string fileName, Encoding? encoding = null, int bufferSize = 1024, bool normalizeLineEndings = true, char endSentinel = '\0')
        {
            Assert.ArgumentNotNullOrEmpty(fileName, nameof(fileName));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));
            _fileName = fileName;
            _line = 1;
            _bufferSize = bufferSize;
            var stream = File.OpenRead(_fileName);
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _currentBuffer = new BufferNode(_bufferSize, _reader, 0);
            _isComplete = _currentBuffer.TotalChars == 0;
            _normalizeLineEndings = normalizeLineEndings;
            _endSentinel = endSentinel;
            if (_isComplete)
                _reader.Dispose();
            _bufferIndex = 0;
        }

        public StreamCharacterSequence(StreamReader reader, string fileName = "", int bufferSize = 1024, bool normalizeLineEndings = true, char endSentinel = '\0')
        {
            Assert.ArgumentNotNull(reader, nameof(reader));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));
            _line = 1;
            _fileName = fileName;
            _bufferIndex = bufferSize;
            _bufferSize = bufferSize;
            _reader = reader;
            _currentBuffer = new BufferNode(_bufferSize, _reader, 0);
            _isComplete = _currentBuffer.TotalChars == 0;
            _normalizeLineEndings = normalizeLineEndings;
            _endSentinel = endSentinel;
            if (_isComplete)
                _reader.Dispose();
            _bufferIndex = 0;
        }

        public StreamCharacterSequence(Stream stream, Encoding? encoding = null, string fileName = "", int bufferSize = 1024, bool normalizeLineEndings = true, char endSentinel = '\0')
        {
            Assert.ArgumentNotNull(stream, nameof(stream));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));
            _line = 1;
            _fileName = fileName ?? "stream";
            _bufferIndex = bufferSize;
            _bufferSize = bufferSize;
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _currentBuffer = new BufferNode(_bufferSize, _reader, 0);
            _isComplete = _currentBuffer.TotalChars == 0;
            _normalizeLineEndings = normalizeLineEndings;
            _endSentinel = endSentinel;
            if (_isComplete)
                _reader.Dispose();

            _bufferIndex = 0;
        }

        public char GetNext()
        {
            var c = GetNextCharRaw(true);
            if (c == _endSentinel)
                return c;
            if (_normalizeLineEndings && c == '\r')
            {
                if (GetNextCharRaw(false) == '\n')
                    GetNextCharRaw(true);
                c = '\n';
            }

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
            if (_normalizeLineEndings && next == '\r')
                next = '\n';
            return next;
        }

        public Location CurrentLocation => new Location(_fileName, _line, _column);

        public bool IsAtEnd => _currentBuffer.TotalChars == 0;

        public int Consumed => _currentBuffer.StartConsumed + _bufferIndex;

        public void Dispose()
        {
            if (!_isComplete)
                _reader.Dispose();
        }

        private char GetNextCharRaw(bool advance)
        {
            // First make sure the buffer is full then return the value if we aren't complete.
            FillBuffer();
            if (_currentBuffer.TotalChars == 0)
                return _endSentinel;
            var c = _currentBuffer.Buffer[_bufferIndex];
            if (advance)
            {
                // _bufferIndex points to the next char to return. Increment it then make sure the
                // buffer is full in case we advance past the end of the current buffer
                // At the end of the stream, FillBuffer will create a new buffer with TotalChars==0
                // which means we're at the end.
                _bufferIndex++;
                FillBuffer();
            }

            return c;
        }

        private void FillBuffer()
        {
            // If there are chars remaining in the current buffer, bail. There's nothing to do
            if (_bufferIndex < _currentBuffer.TotalChars || _currentBuffer.TotalChars == 0)
                return;

            // If this isn't the last buffer in the chain, such as during a rollback, we can advance
            // to the next buffer without reading anything from the stream. Next buffer may be the
            // end buffer, then we're at the end.
            if (_currentBuffer.Next != null)
            {
                _currentBuffer = _currentBuffer.Next;
                _bufferIndex = 0;
                return;
            }

            // Add a new BufferNode to the end of the linked list and update the pointer.
            // Previous nodes will die from GC unless there is a checkpoint holding on to them
            var newBuffer = new BufferNode(_bufferSize, _reader, _currentBuffer.StartConsumed + _currentBuffer.TotalChars);
            _currentBuffer.Next = newBuffer;
            _currentBuffer = newBuffer;
            var remainingChars = _currentBuffer.TotalChars - _bufferIndex;

            if (remainingChars == 0)
            {
                _reader.Dispose();
                _isComplete = true;
            }

            _bufferIndex = 0;
        }

        public ISequenceCheckpoint Checkpoint()
            => new SequenceCheckpoint(this, _currentBuffer, _bufferIndex, _line, _column);

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly StreamCharacterSequence _s;
            private readonly BufferNode _buffer;
            private readonly int _bufferIndex;
            private readonly int _line;
            private readonly int _column;

            public SequenceCheckpoint(StreamCharacterSequence s, BufferNode buffer, int bufferIndex, int line, int column)
            {
                _s = s;
                _buffer = buffer;
                _bufferIndex = bufferIndex;
                _line = line;
                _column = column;
            }

            public int Consumed => _buffer.StartConsumed + _bufferIndex;

            public Location Location => new Location(_s._fileName, _line, _column);

            public void Rewind() => _s.Rewind(_buffer, _bufferIndex, _line, _column);
        }

        private void Rewind(BufferNode buffer, int bufferIndex, int line, int column)
        {
            if (_currentBuffer != buffer)
                _currentBuffer = buffer;
            _bufferIndex = bufferIndex;
            _line = line;
            _column = column;
        }
    }
}
