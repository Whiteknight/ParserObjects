using System;

namespace ParserObjects.Sequences
{
    public class FilterSequence<T> : ISequence<T>
    {
        private readonly ISequence<T> _inputs;
        private readonly Func<T, bool> _filter;

        public FilterSequence(ISequence<T> inputs, Func<T, bool> filter)
        {
            _inputs = inputs;
            _filter = filter;
        }

        public void PutBack(T value)
        {
            if (_filter(value))
                _inputs.PutBack(value);
        }

        public T GetNext()
        {
            DiscardNonMatches();
            return _inputs.GetNext();
        }

        public T Peek()
        {
            DiscardNonMatches();
            return _inputs.Peek();
        }

        public Location CurrentLocation => _inputs.CurrentLocation;

        public bool IsAtEnd
        {
            get
            {
                DiscardNonMatches();
                return _inputs.IsAtEnd;
            }
        }

        private void DiscardNonMatches()
        {
            while (true)
            {
                if (_inputs.IsAtEnd)
                    return;
                var next = _inputs.GetNext();
                if (!_filter(next))
                    continue;
                _inputs.PutBack(next);
                return;
            }
        }
    }
}