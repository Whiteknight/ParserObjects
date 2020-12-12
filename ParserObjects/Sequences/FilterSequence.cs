using System;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Filter a sequence to only return items which match a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterSequence<T> : ISequence<T>
    {
        private readonly ISequence<T> _inputs;
        private readonly Func<T, bool> _predicate;

        private int _consumed;

        public FilterSequence(ISequence<T> inputs, Func<T, bool> predicate)
        {
            Assert.ArgumentNotNull(inputs, nameof(inputs));
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            _inputs = inputs;
            _predicate = predicate;
            _consumed = 0;
        }

        public void PutBack(T value)
        {
            if (_predicate(value))
            {
                _inputs.PutBack(value);
                _consumed--;
            }
        }

        public T GetNext()
        {
            bool hasMore = DiscardNonMatches();

            // If we don't have any values, we are at the end. In this case calling
            // _inputs.GetNext() will return the end sentinel, which is not subject to filtering.
            if (!hasMore)
                return _inputs.GetNext();

            var result = _inputs.GetNext();
            _consumed++;
            return result;
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

        public int Consumed => _consumed;

        private bool DiscardNonMatches()
        {
            while (true)
            {
                if (_inputs.IsAtEnd)
                    return false;
                var next = _inputs.GetNext();
                if (!_predicate(next))
                    continue;
                _inputs.PutBack(next);
                return true;
            }
        }

        public ISequenceCheckpoint Checkpoint() => _inputs.Checkpoint();
    }
}
