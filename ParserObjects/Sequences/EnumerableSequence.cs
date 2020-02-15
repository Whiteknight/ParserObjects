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
        private readonly Func<T> _getEndValue;
        private readonly Stack<T> _putbacks;

        private bool _enumeratorIsAtEnd;
        private int _index;

        public EnumerableSequence(IEnumerable<T> enumerable, Func<T> getEndValue)
            : this(enumerable.GetEnumerator(), getEndValue)
        {
        }

        public EnumerableSequence(IEnumerator<T> enumerator, Func<T> getEndValue)
        {
            _enumerator = enumerator;
            _getEndValue = getEndValue ?? (() => default);
            _putbacks = new Stack<T>();
            _enumeratorIsAtEnd = false;
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
                return _getEndValue();
            _enumeratorIsAtEnd = !_enumerator.MoveNext();
            if (_enumeratorIsAtEnd)
                return _getEndValue();
            _index++;
            return _enumerator.Current;
        }

        public T Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            if (_enumeratorIsAtEnd)
                return _getEndValue();
            _enumeratorIsAtEnd = _enumerator.MoveNext();
            if (_enumeratorIsAtEnd)
                return _getEndValue();
            _index++;
            _putbacks.Push(_enumerator.Current);
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