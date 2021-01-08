using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of characters read from a string.
    /// </summary>
    public class StringCharacterSequence : ISequence<char>
    {
        private readonly string _fileName;
        private readonly string _s;

        private int _index;
        private int _line;
        private int _column;
        private int _consumed;

        public StringCharacterSequence(string s, string fileName = "")
        {
            Assert.ArgumentNotNull(s, nameof(s));
            _s = s;
            _line = 1;
            _column = 0;
            _fileName = fileName;
            _consumed = 0;
        }

        public char GetNext()
        {
            var next = GetNextInternal(true);
            if (next == '\0')
                return next;

            // If \r replace with \n
            // If \r\n advance through the \n and only return \n
            // We only return \n for newlines
            if (next == '\r')
            {
                if (GetNextInternal(false) == '\n')
                    GetNextInternal(true);
                next = '\n';
            }

            // If we have a newline, update the line-tracking.
            if (next == '\n')
            {
                _line++;
                _column = 0;
                _consumed++;
                return next;
            }

            // Bump counts and return
            _column++;
            _consumed++;
            return next;
        }

        private char GetNextInternal(bool advance)
        {
            if (_index >= _s.Length)
                return '\0';
            var value = _s[_index];
            if (advance)
                _index++;
            return value;
        }

        public char Peek()
        {
            var next = GetNextInternal(false);
            if (next == '\r')
                next = '\n';
            return next;
        }

        public Location CurrentLocation => new Location(_fileName, _line, _column);

        public bool IsAtEnd => _index >= _s.Length;

        public int Consumed => _consumed;

        public string GetRemainder()
        {
            if (_index == 0)
                return _s;
            if (_index >= _s.Length)
                return string.Empty;
            return _s.Substring(_index);
        }

        public void Reset()
        {
            _index = 0;
            _line = 0;
            _column = 0;
            _consumed = 0;
        }

        public ISequenceCheckpoint Checkpoint()
            => new StringCheckpoint(this, _index, _line, _column, _consumed);

        private class StringCheckpoint : ISequenceCheckpoint
        {
            private readonly StringCharacterSequence _s;
            private readonly int _index;
            private readonly int _line;
            private readonly int _column;
            private readonly int _consumed;

            public StringCheckpoint(StringCharacterSequence s, int index, int line, int column, int consumed)
            {
                _s = s;
                _index = index;
                _line = line;
                _column = column;
                _consumed = consumed;
            }

            public int Consumed => _consumed;

            public Location Location => new Location(_s._fileName, _line, _column);

            public void Rewind() => _s.Rollback(_index, _line, _column, _consumed);
        }

        private void Rollback(int index, int line, int column, int consumed)
        {
            _index = index;
            _line = line;
            _column = column;
            _consumed = consumed;
        }
    }
}
