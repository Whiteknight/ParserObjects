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
            Assert.ArgumentNotNull(s, nameof(s));
            _s = s;
            _line = 1;
            _column = 0;
            _fileName = fileName;
            _putbacks = new Stack<char>();
            _previousEndOfLineColumns = new AlwaysFullRingBuffer<int>(MaxLineLengthsBufferSize);
        }

        public char GetNext()
        {
            var next = GetNextInternal(true);

            // If \r replace with \n
            // If \n\r advance through the \r
            // We only return \n for newlines
            if (next == '\r')
                next = '\n';
            else if (next == '\n' && GetNextInternal(false) == '\r')
                GetNextInternal(true);

            // If we have a newline, update the line-tracking.
            if (next == '\n')
            {
                _line++;
                _previousEndOfLineColumns.Add(_column);
                _column = 0;
                return next;
            }

            // Bump column and return
            _column++;
            return next;
        }

        private char GetNextInternal(bool advance)
        {
            if (_putbacks.Any())
                return advance ? _putbacks.Pop() : _putbacks.Peek();
            if (_index >= _s.Length)
                return '\0';
            var value = _s[_index];
            if (advance)
                _index++;
            return value;
        }

        public void PutBack(char value)
        {
            // '\0' is the end sentinel, we can't put it back or treat it like a valid value
            if (value == '\0')
                return;
            // If the user insists on PutBack('\r') our _line numbers can get screwed up
            if (value == '\n')
            {
                _line--;
                _previousEndOfLineColumns.MoveBack();
                _column = _previousEndOfLineColumns.GetCurrent();
            }

            // Try to just decrement the pointer if we can, otherwise push it onto the putbacks.
            if (_putbacks.Count == 0 && _index > 0 && _s[_index - 1] == value)
            {
                _index--;
                _column--;
            }
            else
                _putbacks.Push(value);
        }

        public char Peek()
        {
            var next = GetNextInternal(false);
            if (next == '\r')
                next = '\n';
            return next;
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

        public ISequenceCheckpoint Checkpoint()
            => new StringCheckpoint(this, _index, _line, _column, _putbacks.ToArray());

        private class StringCheckpoint : ISequenceCheckpoint
        {
            private readonly StringCharacterSequence _s;
            private readonly int _index;
            private readonly int _line;
            private readonly int _column;
            private readonly char[] _putbacks;

            public StringCheckpoint(StringCharacterSequence s, int index, int line, int column, char[] putbacks)
            {
                _s = s;
                _index = index;
                _line = line;
                _column = column;
                _putbacks = putbacks;
            }

            public void Rewind() => _s.Rollback(_index, _line, _column, _putbacks);
        }

        private void Rollback(int index, int line, int column, char[] putbacks)
        {
            _index = index;
            _line = line;
            _column = column;
            _putbacks.Clear();
            for (int i = putbacks.Length - 1; i >= 0; i--)
                _putbacks.Push(putbacks[i]);
        }
    }
}