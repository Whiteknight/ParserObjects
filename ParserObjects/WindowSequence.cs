using System.Collections.Generic;

namespace ParserObjects
{
    public class WindowSequence<T> : ISequence<T>
    {
        private readonly Stack<T> _window;
        private readonly ISequence<T> _inner;

        public WindowSequence(ISequence<T> inner)
        {
            _inner = inner;
            _window = new Stack<T>();
        }

        public void PutBack(T value)
        {
            if (_window.Peek().Equals(value))
                _window.Pop();
            _inner.PutBack(value);
        }

        public void Rewind()
        {
            while (_window.Count > 0)
                _inner.PutBack(_window.Pop());
        }

        public T GetNext()
        {
            var token = _inner.GetNext();
            _window.Push(token);
            return token;
        }

        public T Peek()
        {
            return _inner.Peek();
        }

        public Location CurrentLocation => _inner.CurrentLocation;
        public bool IsAtEnd => _inner.IsAtEnd;
    }
}