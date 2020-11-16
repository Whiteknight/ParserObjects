using System;
using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Wraps an IEnumerable as an ISequence. Makes the items from the enumerable usable in parse operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class EnumerableSequence<T> : ISequence<T>, IDisposable
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly T _endSentinelValue;
        private readonly Stack<T> _putbacks;

        private Node _current;
        private bool _enumeratorIsAtEnd;
        private int _index;

        public EnumerableSequence(IEnumerable<T> enumerable, Func<T> getEndValue)
            : this(enumerable?.GetEnumerator(), (getEndValue ?? (() => default))())
        {
        }

        public EnumerableSequence(IEnumerator<T> enumerator, Func<T> getEndValue)
            : this(enumerator, (getEndValue ?? (() => default))())
        {
        }

        public EnumerableSequence(IEnumerable<T> enumerable, T endValue)
            : this(enumerable?.GetEnumerator(), endValue)
        {
        }

        public EnumerableSequence(IEnumerator<T> enumerator, T endValue)
        {
            Assert.ArgumentNotNull(enumerator, nameof(enumerator));
            _enumerator = enumerator;
            _enumeratorIsAtEnd = !_enumerator.MoveNext();
            _endSentinelValue = endValue;
            _putbacks = new Stack<T>();

            // The first item in the linked list will be an end sentinel, which we will probably
            // never look at, but we need it for logic later.
            _current = new Node { Value = _endSentinelValue, Next = null };
            if (!_enumeratorIsAtEnd)
            {
                var node = new Node { Value = _enumerator.Current, Next = null };
                _current.Next = node;
            }
            _index = 0;
        }

        private class Node
        {
            public T Value { get; set; }
            public Node Next { get; set; }
        }

        public void PutBack(T value)
        {
            _putbacks.Push(value);
            _index--;
        }

        public T GetNext()
        {
            if (_putbacks.Count > 0)
            {
                _index++;
                return _putbacks.Pop();
            }

            // We should always have a value queued up next, unless we're at the end. So if we see
            // Next==null, we can bail
            if (_current.Next == null)
                return _endSentinelValue;

            _current = _current.Next;
            var value = _current.Value;
            _index++;

            // If we already have more in the queue, we can just return this value and not update
            // anything else
            if (_current.Next != null)
                return value;

            // At this point, there's nothing else in the queue. We need to try to advance the
            // enumerator and add a new value to the queue. If the enumerator is at end, there will
            // be nothing queued.

            _enumeratorIsAtEnd = !_enumerator.MoveNext();
            if (_enumeratorIsAtEnd)
                return value;

            Debug.Assert(_current.Next == null);
            var node = new Node { Value = _enumerator.Current, Next = null };
            _current.Next = node;
            return value;
        }

        public T Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            if (_current.Next == null)
                return _endSentinelValue;
            return _current.Next.Value;
        }

        public Location CurrentLocation => new Location(null, 1, _index);

        public bool IsAtEnd => _enumeratorIsAtEnd && _current.Next == null && _putbacks.Count == 0;

        public void Dispose()
        {
            _enumerator?.Dispose();
        }

        public ISequenceCheckpoint Checkpoint() => new SequenceCheckpoint(this, _current, _index, _enumeratorIsAtEnd, _putbacks.ToArray());

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly EnumerableSequence<T> _s;
            private readonly Node _node;
            private readonly int _index;
            private readonly bool _enumeratorIsAtEnd;
            private readonly T[] _putbacks;

            public SequenceCheckpoint(EnumerableSequence<T> s, Node node, int index, bool enumeratorIsAtEnd, T[] putbacks)
            {
                _s = s;
                _node = node;
                _index = index;
                _enumeratorIsAtEnd = enumeratorIsAtEnd;
                _putbacks = putbacks;
            }

            public void Rewind() => _s.Rewind(_node, _index, _enumeratorIsAtEnd, _putbacks);
        }

        private void Rewind(Node node, int index, bool isAtEnd, T[] putbacks)
        {
            _current = node;
            _enumeratorIsAtEnd = isAtEnd;
            _putbacks.Clear();
            _index = index;
            for (int i = putbacks.Length - 1; i >= 0; i--)
                _putbacks.Push(putbacks[i]);
        }
    }
}