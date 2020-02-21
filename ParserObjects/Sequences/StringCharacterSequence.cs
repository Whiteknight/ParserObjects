using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of characters read from a string
    /// </summary>
    public class StringCharacterSequence : ISequence<char>
    {
        // TODO: Newline characters should be configurable and platform-specific
        private const int MaxLineLengthsBufferSize = 5;

        private readonly string _fileName;
        private readonly string _s;
        private readonly Stack<char> _putbacks;
        private readonly AlwaysFullRingBuffer<int> _previousEndOfLineColumns;

        private int _index;
        private int _line;
        private int _column;

        public StringCharacterSequence(string s, string fileName = null)
        {
            _s = s;
            _fileName = fileName;
            _putbacks = new Stack<char>();
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
        }

        public char GetNext()
        {
            var next = GetNextInternal();
            if (next == '\n')
            {
                _line++;
                _previousEndOfLineColumns.Add(_column);
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
            // '\0' is the end sentinel, we can't put it back or treat it like a valid value
            if (value == '\0')
                return;
            if (value == '\n')
            {
                _line--;
                _previousEndOfLineColumns.MoveBack();
                _column = _previousEndOfLineColumns.GetCurrent();
            }

            // TODO: If the putback is in the string, we should be able to just decrement
            // the index
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

        public string GetRemainder()
        {
            if (_putbacks.Count == 0)
                return GetStringBufferRemainder();

            // Little optimization, we don't need a StringBuilder if we only have one putback
            if (_putbacks.Count == 1)
                return _putbacks.Peek().ToString() + GetStringBufferRemainder();

            var builder = new StringBuilder();
            var elements = new char[_putbacks.Count];
            _putbacks.CopyTo(elements, 0);
            for (int i = 0; i < elements.Length; i++)
                builder.Append(elements[i]);
            builder.Append(GetStringBufferRemainder());
            return builder.ToString();
        }

        public void Reset()
        {
            _index = 0;
            _line = 0;
            _column = 0;
            _putbacks.Clear();
        }

        private string GetStringBufferRemainder()
        {
            if (_index == 0)
                return _s;
            if (_index >= _s.Length)
                return string.Empty;
            return _s.Substring(_index);
        }
    }
}