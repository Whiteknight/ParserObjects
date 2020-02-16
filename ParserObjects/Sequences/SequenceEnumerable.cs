using System;
using System.Collections;
using System.Collections.Generic;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Adaptor to convert ISequence to IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SequenceEnumerable<T> : IEnumerable<T>
    {
        private readonly ISequence<T> _inputs;

        public SequenceEnumerable(ISequence<T> inputs)
        {
            _inputs = inputs;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => new Enumerator(_inputs);

        private class Enumerator : IEnumerator<T>
        {
            private readonly ISequence<T> _input;

            public Enumerator(ISequence<T> input)
            {
                _input = input;
            }

            public bool MoveNext()
            {
                var atEnd = _input.IsAtEnd;
                Current = _input.GetNext();
                return !atEnd;
            }

            public void Reset() => throw new NotImplementedException();

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public T Current { get; private set;  }
        }
    }
}
