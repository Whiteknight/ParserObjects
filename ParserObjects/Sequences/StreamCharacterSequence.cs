using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of characters read from a Stream, such as from a file
    /// </summary>
    public class StreamCharacterSequence : ISequence<char>, IDisposable
    {
        private const int BufferSize = 128;
        private const int MaxLineLengthsBufferSize = 5;

        private readonly string _fileName;
        private readonly StreamReader _reader;
        private readonly Stack<char> _putbacks;
        private readonly char[] _buffer;
        private readonly AlwaysFullRingBuffer<int> _previousEndOfLineColumns;

        private bool _isComplete;
        private int _remainingChars;
        private int _bufferIndex;
        private int _line;
        private int _column;

        public StreamCharacterSequence(string fileName, Encoding encoding = null)
        {
            _fileName = fileName;
            _putbacks = new Stack<char>();
            _bufferIndex = BufferSize;
            _buffer = new char[BufferSize];
            var stream = File.OpenRead(_fileName);
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
        }

        public StreamCharacterSequence(StreamReader reader, string fileName = null)
        {
            _fileName = fileName;
            _putbacks = new Stack<char>();
            _bufferIndex = BufferSize;
            _buffer = new char[BufferSize];
            _reader = reader;
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
        }

        public StreamCharacterSequence(Stream stream, Encoding encoding = null, string fileName = null)
        {
            _fileName = fileName ?? "stream";
            _putbacks = new Stack<char>();
            _bufferIndex = BufferSize;
            _buffer = new char[BufferSize];
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
        }

        public char GetNext()
        {
            var c = GetNextCharRaw(true);
            if (c == '\0')
                return c;
            if (c == '\r')
                c = '\n';
            else if (c == '\n' && GetNextCharRaw(false) == '\r')
                GetNextCharRaw(true);

            _column++;
            if (c == '\n')
            {
                _line++;
                _previousEndOfLineColumns.Add(_column - 1);
                _column = 0;
            }

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
                _previousEndOfLineColumns.MoveBack();
                _column = _previousEndOfLineColumns.GetCurrent();
            }
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
            if (_isComplete || _remainingChars == 0 || _bufferIndex >= BufferSize)
                return '\0';
            var c = _buffer[_bufferIndex];
            if (advance)
            {
                _bufferIndex++;
                _remainingChars--;
            }

            return c;
        }

        private void FillBuffer()
        {
            if (_remainingChars != 0 && _bufferIndex < BufferSize)
                return;
            _remainingChars = _reader.Read(_buffer, 0, BufferSize);
            if (_remainingChars == 0)
                _isComplete = true;
            _bufferIndex = 0;
        }
    }
}
