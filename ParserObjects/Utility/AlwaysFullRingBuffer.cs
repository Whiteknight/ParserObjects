namespace ParserObjects.Utility
{
    // Simplified ring-buffer implementation which assumes the buffer is always full so we don't need to
    // keep track of a half-full buffer with a start and end pointer.
    public class AlwaysFullRingBuffer<T>
    {
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