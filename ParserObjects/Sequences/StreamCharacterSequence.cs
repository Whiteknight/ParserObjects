using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of characters read from a Stream, such as from a file
    /// </summary>
    public class StreamCharacterSequence : ISequence<char>, IDisposable
    {
        private const int BufferSize = 128;
        private readonly string _fileName;
        private readonly StreamReader _reader;
        private readonly Stack<char> _putbacks;
        private readonly char[] _buffer;
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
        }

        public StreamCharacterSequence(StreamReader reader, string fileName = null)
        {
            _fileName = fileName;
            _putbacks = new Stack<char>();
            _bufferIndex = BufferSize;
            _buffer = new char[BufferSize];
            _reader = reader;
        }

        public StreamCharacterSequence(Stream stream, Encoding encoding = null, string fileName = null)
        {
            _fileName = fileName ?? "stream";
            _putbacks = new Stack<char>();
            _bufferIndex = BufferSize;
            _buffer = new char[BufferSize];
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
        }

        public char GetNext()
        {
            var c = GetNextCharRaw();
            if (c == '\0')
                return c;
            if (c == '\n')
            {
                _line++;
                _column = 0;
            }
            else
                _column++;

            return c;
        }

        public void PutBack(char value)
        {
            _putbacks.Push(value);
            if (value == '\n')
                _line--;
        }

        public char Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            var next = GetNext();
            PutBack(next);
            return next;
        }

        public Location CurrentLocation => new Location(_fileName, _line, _column);
        public bool IsAtEnd => _putbacks.Count == 0 && _isComplete;

        public void Dispose()
        {
            _reader?.Dispose();
        }

        private char GetNextCharRaw()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Pop();
            if (_isComplete)
                return '\0';
            if (_remainingChars == 0 || _bufferIndex >= BufferSize)
                FillBuffer();
            if (_isComplete || _remainingChars == 0 || _bufferIndex >= BufferSize)
                return '\0';
            var c = _buffer[_bufferIndex];
            _bufferIndex++;
            _remainingChars--;
            return c;
        }

        private void FillBuffer()
        {
            _remainingChars = _reader.Read(_buffer, 0, BufferSize);
            if (_remainingChars == 0)
                _isComplete = true;
            _bufferIndex = 0;
        }
    }
}
