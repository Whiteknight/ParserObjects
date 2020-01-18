using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of characters read from a string
    /// </summary>
    public class StringCharacterSequence : ISequence<char>
    {
        private readonly string _fileName;
        private readonly string _s;
        private readonly Stack<char> _putbacks;
        private int _index;
        private int _line;
        private int _column;
        private int _previousEndOfLineColumn;

        public StringCharacterSequence(string s, string fileName = null)
        {
            _s = s;
            _fileName = fileName;
            _putbacks = new Stack<char>();
        }

        public char GetNext()
        {
            var next = GetNextInternal();
            if (next == '\n')
            {
                _line++;
                _previousEndOfLineColumn = _column;
                _column = 0;
                return next;
            }

            _column++;
            return next;
        }

        private char GetNextInternal()
        {
            if (_putbacks.Any())
                return _putbacks.Pop();
            return _index >= _s.Length ? '\0' : _s[_index++];
        }

        public void PutBack(char value)
        {
            if (value == '\n')
            {
                _line--;
                _column = _previousEndOfLineColumn;
            }

            _putbacks.Push(value);
        }

        public char Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            if (_index >= _s.Length)
                return '\0';
            return _s[_index];
        }

        public Location CurrentLocation => new Location(_fileName, _line, _column);

        public bool IsAtEnd => _putbacks.Count == 0 && _index >= _s.Length;
    }
}