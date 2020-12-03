using System.Diagnostics;

namespace ParserObjects.Utility
{
    /// <summary>
    /// Simplified ring-buffer ('circular buffer') implementation which makes several assumptions
    /// for simplicity and performance. The buffer is always considered full (reading more data
    /// than has been written will return default values) and values are not modifiable. This class
    /// is not concurrency-safe.
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
                // near IntMax/2 with the same modulo position in the buffer.
                var current = _index % _size;
                ResetIndexToMidRange(current);
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
                ResetIndexToMidRange(current);
            }

            _index--;
        }

        private void ResetIndexToMidRange(int current)
        {
            // For example, we have a number space of 100 and a _size of 9. 100/9=11, so there are
            // 11 module ranges in the number space. ratio>>1 gives us 5. So we're looking for a
            // position in range 5 where the module is current. We get that by jumping to
            // position (_size*5), which is by definition position 0 after module _size, then we
            // add the current index. Now ((_size*ratio)+current)%_size == current, and we are
            // far away from the edge of number space.
            var ratio = int.MaxValue / _size;
            ratio = ratio >> 1;
            _index = (_size * ratio) + current;
            Debug.Assert(_index % _size == current, "Index changed after reset");
        }

        public T GetCurrent()
        {
            var index = _index % _size;
            return _buffer[index];
        }

        public T[] ToArray()
        {
            var newArray = new T[_size];
            var startIndex = _index % _size;
            for (int i = 0; i < _size; i++)
            {
                var currentIdex = (i + startIndex) % _size;
                newArray[i] = _buffer[currentIdex];
            }

            return newArray;
        }

        public void OverwriteFromArray(T[] array)
        {
            Debug.Assert(array.Length == _size, "The array is not the correct size for this buffer");

            // Copy the array over, starting at buffer position 0 (regardless of where _index is
            // right now)
            for (int i = 0; i < _size; i++)
            {
                _buffer[i] = array[i];
                _index = 0;
            }

            // Reset _index to be in the middle of the number range such that it is pointing to
            // position 0 after the modulo. That way we can just keep moving from the start of
            // the buffer
            ResetIndexToMidRange(0);
        }
    }
}