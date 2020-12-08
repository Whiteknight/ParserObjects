using System;
using System.Collections.Generic;
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
        private const int MaxLineLengthsBufferSize = 5;

        private readonly int _bufferSize;
        private readonly string _fileName;
        private readonly StreamReader _reader;
        private readonly Stack<char> _putbacks;
        private readonly AlwaysFullRingBuffer<int> _previousEndOfLineColumns;

        private BufferNode _currentBuffer;
        private bool _isComplete;
        private int _remainingChars;
        private int _bufferIndex;
        private int _line;
        private int _column;
        private int _consumed;

        private class BufferNode
        {
            public char[] Buffer { get; }
            public BufferNode Next { get; set; }
            public int TotalChars { get; }

            public BufferNode(int bufferSize, StreamReader reader)
            {
                Buffer = new char[bufferSize];
                TotalChars = reader.Read(Buffer, 0, bufferSize);
                Next = null;
            }
        }

        public StreamCharacterSequence(string fileName, Encoding encoding = null, int bufferSize = 1024)
        {
            Assert.ArgumentNotNullOrEmpty(fileName, nameof(fileName));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));
            _fileName = fileName;
            _line = 1;
            _putbacks = new Stack<char>();
            _bufferIndex = bufferSize;
            _bufferSize = bufferSize;
            var stream = File.OpenRead(_fileName);
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
            _consumed = 0;
            FillBuffer();
        }

        public StreamCharacterSequence(StreamReader reader, string fileName = null, int bufferSize = 1024)
        {
            Assert.ArgumentNotNull(reader, nameof(reader));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));
            _line = 1;
            _fileName = fileName;
            _putbacks = new Stack<char>();
            _bufferIndex = bufferSize;
            _bufferSize = bufferSize;
            _reader = reader;
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
            _consumed = 0;
            FillBuffer();
        }

        public StreamCharacterSequence(Stream stream, Encoding encoding = null, string fileName = null, int bufferSize = 1024)
        {
            Assert.ArgumentNotNull(stream, nameof(stream));
            Assert.ArgumentGreaterThan(bufferSize, 0, nameof(bufferSize));
            _line = 1;
            _fileName = fileName ?? "stream";
            _putbacks = new Stack<char>();
            _bufferIndex = bufferSize;
            _bufferSize = bufferSize;
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
            _consumed = 0;
            FillBuffer();
        }

        public char GetNext()
        {
            var c = GetNextCharRaw(true);
            if (c == '\0')
                return c;
            if (c == '\r')
            {
                if (GetNextCharRaw(false) == '\n')
                    GetNextCharRaw(true);
                c = '\n';
            }

            if (c == '\n')
            {
                _line++;
                _previousEndOfLineColumns.Add(_column);
                _column = 0;
                return c;
            }

            _column++;
            _consumed++;
            return c;
        }

        public void PutBack(char value)
        {
            if (value == '\0')
                return;
            _putbacks.Push(value);
            if (value == '\n')
            {
                _line--;
                _column = _previousEndOfLineColumns.GetCurrent();
                _previousEndOfLineColumns.MoveBack();
            }

            _consumed--;
        }

        public char Peek()
        {
            var next = GetNextCharRaw(false);
            if (next == '\r')
                next = '\n';
            return next;
        }

        public Location CurrentLocation => new Location(_fileName, _line, _column);

        public bool IsAtEnd => _putbacks.Count == 0 && _isComplete;

        public int Consumed => _consumed;

        public void Dispose()
        {
            _reader?.Dispose();
        }

        private char GetNextCharRaw(bool advance)
        {
            if (_putbacks.Count > 0)
                return advance ? _putbacks.Pop() : _putbacks.Peek();
            if (_isComplete)
                return '\0';

            FillBuffer();
            if (_isComplete || _remainingChars == 0 || _bufferIndex >= _bufferSize)
                return '\0';
            var c = _currentBuffer.Buffer[_bufferIndex];
            if (advance)
            {
                _bufferIndex++;
                _remainingChars--;
                if (_remainingChars == 0)
                    FillBuffer();
            }

            return c;
        }

        private void FillBuffer()
        {
            if (_remainingChars != 0 && _bufferIndex < _bufferSize)
                return;

            // If this isn't the last buffer in the chain, such as during a rollback, we can advance
            // to the next buffer without reading anything from the stream.
            if (_currentBuffer?.Next != null)
            {
                _currentBuffer = _currentBuffer.Next;
                _remainingChars = _currentBuffer.TotalChars;
                _bufferIndex = 0;
                _isComplete = _remainingChars == 0;
                return;
            }

            // Add a new BufferNode to the end of the linked list and update the pointer.
            // Previous nodes will die from GC unless there is a checkpoint holding on to them
            var newBuffer = new BufferNode(_bufferSize, _reader);
            if (_currentBuffer != null)
                _currentBuffer.Next = newBuffer;
            _currentBuffer = newBuffer;
            _remainingChars = _currentBuffer.TotalChars;

            if (_remainingChars == 0)
                _isComplete = true;
            _bufferIndex = 0;
        }

        public ISequenceCheckpoint Checkpoint()
        {
            if (_currentBuffer == null)
                FillBuffer();
            return new SequenceCheckpoint(this, _currentBuffer, _remainingChars, _bufferIndex, _line, _column, _putbacks.ToArray(), _previousEndOfLineColumns.ToArray(), _consumed);
        }

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly StreamCharacterSequence _s;
            private readonly BufferNode _buffer;
            private readonly int _remainingChars;
            private readonly int _bufferIndex;
            private readonly int _line;
            private readonly int _column;
            private readonly char[] _putbacks;
            private readonly int[] _lineEndCols;
            private readonly int _consumed;

            public SequenceCheckpoint(StreamCharacterSequence s, BufferNode buffer, int remainingChars, int bufferIndex, int line, int column, char[] putbacks, int[] lineEndCols, int consumed)
            {
                _s = s;
                _buffer = buffer;
                _remainingChars = remainingChars;
                _bufferIndex = bufferIndex;
                _line = line;
                _column = column;
                _putbacks = putbacks;
                _lineEndCols = lineEndCols;
                _consumed = consumed;
            }

            public void Rewind() => _s.Rewind(_buffer, _remainingChars, _bufferIndex, _line, _column, _putbacks, _lineEndCols, _consumed);
        }

        private void Rewind(BufferNode buffer, int remainingChars, int bufferIndex, int line, int column, char[] putbacks, int[] lineEndCols, int consumed)
        {
            if (_currentBuffer != buffer)
                _currentBuffer = buffer;
            _remainingChars = remainingChars;
            _bufferIndex = bufferIndex;
            _isComplete = false;
            _line = line;
            _column = column;
            _putbacks.Clear();
            for (int i = putbacks.Length - 1; i >= 0; i--)
                _putbacks.Push(putbacks[i]);
            _previousEndOfLineColumns.OverwriteFromArray(lineEndCols);
            _consumed = consumed;
        }
    }
}
