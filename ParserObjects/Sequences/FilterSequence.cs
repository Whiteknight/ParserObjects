using System;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Filter a sequence to only return items which match a predicate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterSequence<T> : ISequence<T>
    {
        private readonly ISequence<T> _inputs;
        private readonly Func<T, bool> _predicate;

        public FilterSequence(ISequence<T> inputs, Func<T, bool> predicate)
        {
            Assert.ArgumentNotNull(inputs, nameof(inputs));
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            _inputs = inputs;
            _predicate = predicate;
        }

        public void PutBack(T value)
        {
            if (_predicate(value))
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
                if (!_predicate(next))
                    continue;
                _inputs.PutBack(next);
                return;
            }
        }
    }
}