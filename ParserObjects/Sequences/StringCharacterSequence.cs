using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence of characters read from a string.
    /// </summary>
    public class StringCharacterSequence : ISequence<char>
    {
        private readonly string _fileName;
        private readonly bool _normalizeLineEndings;
        private readonly string _s;
        private readonly char _endSentinel;
        private readonly SequenceStatistics _stats;

        private int _index;
        private int _line;
        private int _column;
        private int _consumed;

        public StringCharacterSequence(string s, string fileName = "", bool normalizeLineEndings = true, char endSentinel = '\0')
        {
            Assert.ArgumentNotNull(s, nameof(s));
            _stats = new SequenceStatistics();
            _s = s;
            _line = 1;
            _column = 0;
            _fileName = fileName;
            _normalizeLineEndings = normalizeLineEndings;
            _consumed = 0;
            _endSentinel = endSentinel;
        }

        public char GetNext()
        {
            var next = GetNextInternal(true);
            if (next == _endSentinel)
                return next;

            // If line endings are normalized, we replace \r -> \n and \r\n -> \n
            if (_normalizeLineEndings && next == '\r')
            {
                if (GetNextInternal(false) == '\n')
                    GetNextInternal(true);
                next = '\n';
            }

            _stats.ItemsRead++;

            // If we have a newline, update the line-tracking.
            // TODO: What do we do about systems that only use \r for newlines? Right now we ignore
            // it if normalization is off.
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
                return _endSentinel;
            var value = _s[_index];
            if (advance)
                _index++;
            return value;
        }

        public char Peek()
        {
            var next = GetNextInternal(false);
            if (_normalizeLineEndings && next == '\r')
                next = '\n';
            _stats.ItemsPeeked++;
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
        {
            _stats.CheckpointsCreated++;
            return new StringCheckpoint(this, _index, _line, _column, _consumed);
        }

        private class StringCheckpoint : ISequenceCheckpoint
        {
            private readonly StringCharacterSequence _s;
            private readonly int _index;
            private readonly int _line;
            private readonly int _column;

            public StringCheckpoint(StringCharacterSequence s, int index, int line, int column, int consumed)
            {
                _s = s;
                _index = index;
                _line = line;
                _column = column;
                Consumed = consumed;
            }

            public int Consumed { get; }

            public Location Location => new Location(_s._fileName, _line, _column);

            public void Rewind() => _s.Rewind(_index, _line, _column, Consumed);
        }

        private void Rewind(int index, int line, int column, int consumed)
        {
            _stats.Rewinds++;
            _stats.RewindsToCurrentBuffer++;
            _index = index;
            _line = line;
            _column = column;
            _consumed = consumed;
        }

        public ISequenceStatistics GetStatistics() => _stats.Snapshot();
    }
}
