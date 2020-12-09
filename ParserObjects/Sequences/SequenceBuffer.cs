using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Adaptor to represent a sequence as an indexable buffer like an array or list. Care must
    /// be taken to ensure unused items from the buffer are returned to the sequence after work
    /// on the buffer has completed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SequenceBuffer<T>
    {
        private readonly ISequence<T> _input;
        private readonly List<T> _buffer;
        private readonly int _offset;

        public SequenceBuffer(ISequence<T> input)
        {
            Assert.ArgumentNotNull(input, nameof(input));
            _input = input;
            _buffer = new List<T>();
            _offset = 0;
        }

        private SequenceBuffer(ISequence<T> input, List<T> buffer, int offset)
        {
            _input = input;
            _buffer = buffer;
            _offset = offset;
        }

        public SequenceBuffer<T> CopyFrom(int i) => new SequenceBuffer<T>(_input, _buffer, i);

        public T[] Capture(int i)
        {
            var tokens = _buffer.Skip(_offset).Take(i).ToArray();
            for (int j = _buffer.Count - 1; j >= i; j--)
                _input.PutBack(_buffer[j]);
            return tokens;
        }

        public T this[int index]
        {
            get
            {
                var realIndex = _offset + index;
                FillUntil(realIndex);
                if (realIndex >= _buffer.Count)
                    return default;
                return _buffer[realIndex];
            }
        }

        public bool IsPastEnd(int index)
        {
            var realIndex = _offset + index;
            FillUntil(realIndex);
            if (realIndex >= _buffer.Count)
                return true;
            return false;
        }

        private void FillUntil(int i)
        {
            if (_buffer.Count > i)
                return;
            int numToGet = _buffer.Count - i + 1;
            for (var j = 0; j < numToGet; j++)
            {
                if (_input.IsAtEnd)
                    break;
                _buffer.Add(_input.GetNext());
            }
        }
    }
}
