using System;
using System.Collections.Generic;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Wraps an IEnumerable as an ISequence. Makes the items from the enumerable usable in parse operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableSequence<T> : ISequence<T>, IDisposable
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly T _endValue;
        private readonly Stack<T> _putbacks;

        private bool _enumeratorIsAtEnd;
        private int _index;

        public EnumerableSequence(IEnumerable<T> enumerable, Func<T> getEndValue)
            : this(enumerable.GetEnumerator(), (getEndValue ?? (() => default))())
        {
        }

        public EnumerableSequence(IEnumerator<T> enumerator, Func<T> getEndValue)
            : this(enumerator, (getEndValue ?? (() => default))())
        {
        }

        public EnumerableSequence(IEnumerable<T> enumerable, T endValue)
            : this(enumerable.GetEnumerator(), endValue)
        {
        }

        public EnumerableSequence(IEnumerator<T> enumerator, T endValue)
        {
            _enumerator = enumerator;
            _enumeratorIsAtEnd = !_enumerator.MoveNext();
            _endValue = endValue;
            _putbacks = new Stack<T>();
            _index = 0;
        }

        public void PutBack(T value)
        {
            _putbacks.Push(value);
            _index--;
            if (_index < 0)
                _index = 0;
        }

        public T GetNext()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Pop();
            if (_enumeratorIsAtEnd)
                return _endValue;
            var value = _enumerator.Current;
            _enumeratorIsAtEnd = !_enumerator.MoveNext();
            _index++;
            return value;
        }

        public T Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            if (_enumeratorIsAtEnd)
                return _endValue;
            return _enumerator.Current;
        }

        public Location CurrentLocation => new Location(null, 0, _index - _putbacks.Count);

        public bool IsAtEnd => _putbacks.Count == 0 && _enumeratorIsAtEnd;

        public void Dispose()
        {
            _enumerator?.Dispose();
        }
    }
}