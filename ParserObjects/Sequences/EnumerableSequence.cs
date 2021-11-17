﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Wraps an IEnumerable as an ISequence. Makes the items from the enumerable usable in parse
    /// operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class EnumerableSequence<T> : ISequence<T?>, IDisposable
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly T? _endSentinelValue;

        private SequenceStatistics _stats;
        private Node _current;
        private bool _enumeratorIsAtEnd;
        private int _index;
        private int _consumed;

        public EnumerableSequence(IEnumerable<T> enumerable, Func<T?> getEndValue)
            : this(enumerable.GetEnumerator(), (getEndValue ?? (() => default))())
        {
        }

        public EnumerableSequence(IEnumerator<T> enumerator, Func<T?> getEndValue)
            : this(enumerator, (getEndValue ?? (() => default))())
        {
        }

        public EnumerableSequence(IEnumerable<T> enumerable, T? endValue)
            : this(enumerable.GetEnumerator(), endValue)
        {
        }

        public EnumerableSequence(IEnumerator<T> enumerator, T? endValue)
        {
            Assert.ArgumentNotNull(enumerator, nameof(enumerator));
            _enumerator = enumerator;
            _enumeratorIsAtEnd = !_enumerator.MoveNext();
            _endSentinelValue = endValue;

            // The first item in the linked list will be an end sentinel, which we will probably
            // never look at, but we need it for logic later.
            _current = new Node { Value = _endSentinelValue, Next = null };
            if (!_enumeratorIsAtEnd)
                _current.Next = new Node { Value = _enumerator.Current, Next = null };

            _index = 0;
            _consumed = 0;

            _stats = default;
        }

        private class Node
        {
            public T? Value { get; set; }
            public Node? Next { get; set; }
        }

        public T? GetNext()
        {
            // We should always have a value queued up next, unless we're at the end. So if we see
            // Next==null, we can bail
            if (_current.Next == null)
                return _endSentinelValue;

            _current = _current.Next;
            var value = _current.Value;
            _index++;
            _consumed++;

            // If we already have more in the queue, we can just return this value and not update
            // anything else
            if (_current.Next != null)
            {
                _stats.ItemsRead++;
                return value;
            }

            // At this point, there's nothing else in the queue. See about getting the next value
            // from the enumerator, if there is anything left.

            if (_enumeratorIsAtEnd)
                return _endSentinelValue;

            _stats.ItemsRead++;
            _enumeratorIsAtEnd = !_enumerator.MoveNext();
            if (_enumeratorIsAtEnd)
            {
                _enumerator.Dispose();
                return value;
            }

            Debug.Assert(_current.Next == null, "The linked list is broken");
            _current.Next = new Node { Value = _enumerator.Current, Next = null };
            return value;
        }

        public T? Peek()
        {
            if (_current.Next == null)
                return _endSentinelValue;
            _stats.ItemsPeeked++;
            return _current.Next.Value;
        }

        public Location CurrentLocation => new Location(string.Empty, 1, _index);

        public bool IsAtEnd => _enumeratorIsAtEnd && _current.Next == null;

        public int Consumed => _consumed;

        public void Dispose()
        {
            _enumerator?.Dispose();
        }

        public ISequenceCheckpoint Checkpoint()
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(this, _current, _index, _consumed, _enumeratorIsAtEnd);
        }

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly EnumerableSequence<T> _s;
            private readonly Node _node;
            private readonly int _index;
            private readonly bool _enumeratorIsAtEnd;

            public SequenceCheckpoint(EnumerableSequence<T> s, Node node, int index, int consumed, bool enumeratorIsAtEnd)
            {
                _s = s;
                _node = node;
                _index = index;
                _enumeratorIsAtEnd = enumeratorIsAtEnd;
                Consumed = consumed;
            }

            public int Consumed { get; }

            public Location Location => new Location(string.Empty, 1, _index);

            public void Rewind() => _s.Rewind(_node, _index, Consumed, _enumeratorIsAtEnd);
        }

        private void Rewind(Node node, int index, int consumed, bool isAtEnd)
        {
            _stats.Rewinds++;
            _current = node;
            _enumeratorIsAtEnd = isAtEnd;
            _index = index;
            _consumed = consumed;
        }

        public ISequenceStatistics GetStatistics() => _stats;
    }
}
