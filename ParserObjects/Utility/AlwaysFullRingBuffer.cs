using System.Diagnostics;

namespace ParserObjects.Utility
{
    /// <summary>
    /// Simplified ring-buffer implementation which makes several assumptions for simplicity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AlwaysFullRingBuffer<T>
    {
        // By assuming that the buffer starts out full with default values, and not caring if we rewind a
        // a full loop, we can simplify the implementation and only use a single index and simplified check
        // logic
        private readonly int _size;
        private readonly T[] _buffer;

        private int _index;

        public AlwaysFullRingBuffer(int size, T defaultValue = default)
            : this(size, defaultValue, int.MaxValue >> 1)
        {
            // Start _index in the middle of the available number-space so we can move forward and backwards
            // arbitrarily far (it's possible, though unlikely, that we process over a billion input values)
        }

        public AlwaysFullRingBuffer(int size, T defaultValue, int startIndex)
        {
            Assert.ArgumentGreaterThan(size, 0, nameof(size));
            _size = size;
            _buffer = new T[size];
            for (int i = 0; i < size; i++)
                _buffer[i] = defaultValue;
            _index = startIndex;
        }

        public void Add(T value)
        {
            MoveForward();
            var index = _index % _size;
            _buffer[index] = value;
        }

        public void MoveForward()
        {
            if (_index == int.MaxValue)
            {
                // If we are at max value and would roll-over, instead move _index to a position 
                // near IntMax/4 with the same modulo position in the buffer.
                var current = _index % _size;
                var ratio = int.MaxValue / _size;
                ratio = ratio >> 4;
                _index = (_size * ratio) + current;
                Debug.Assert(_index % _size == current);
            }
            _index++;
        }

        public void MoveBack()
        {
            // If we are at 0 and would roll-over to negative, instead reset to an index near the
            // middle of number-space that has the same relative modulo position in the buffer.
            if (_index == 0)
            {
                var current = _index % _size;
                var ratio = int.MaxValue / _size;
                ratio = ratio >> 2;
                _index = (_size * ratio) + current;
                Debug.Assert(_index % _size == current);
            }

            _index--;
        }

        public T GetCurrent()
        {
            var index = _index % _size;
            return _buffer[index];
        }
    }
}