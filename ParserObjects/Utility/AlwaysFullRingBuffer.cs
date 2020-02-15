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

        public AlwaysFullRingBuffer(int size)
        {
            _size = size;
            _buffer = new T[size];
            for (int i = 0; i < size; i++)
                _buffer[i] = default;
            _index = 0;
        }

        public void Add(T value)
        {
            var index = _index % _size;
            _buffer[index] = value;
            _index++;
        }

        public void MoveBack()
        {
            _index--;
        }

        public T GetCurrent()
        {
            var index = _index % _size;
            return _buffer[index];
        }
    }
}