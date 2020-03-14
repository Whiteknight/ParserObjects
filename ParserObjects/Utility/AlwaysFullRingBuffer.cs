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
        {
            Assert.ArgumentGreaterThan(size, 0, nameof(size));
            _size = size;
            _buffer = new T[size];
            for (int i = 0; i < size; i++)
                _buffer[i] = defaultValue;
            // Start _index in the middle of the available number-space so we can move forward and backwards
            // arbitrarily far (it's possible, though unlikely, that we process over a billion input values)
            _index = int.MaxValue >> 1;
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
                // If we are at max value and would roll-over, instead drop _index to IntMax/4 (since we
                // are obviously adding much faster than we are putting-back so it wouldn't make sense to
                // just go to the middle) and then hunt for the next index that puts us at the same 
                // relative position in the buffer. Then we can continue.
                var current = _index % _size;
                _index = int.MaxValue >> 2;
                while (_index % _size != current)
                    _index--;
            }
            _index++;
        }

        public void MoveBack()
        {
            // If we are at 0 and would roll-over to negative, instead reset to the middle of the
            // number-space and then hunt for an index that would put us back to the same relative position
            // in the buffer. Then we can continue
            if (_index == 0)
            {
                var current = _index % _size;
                _index = int.MaxValue >> 1;
                while (_index % _size != current)
                    _index++;
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