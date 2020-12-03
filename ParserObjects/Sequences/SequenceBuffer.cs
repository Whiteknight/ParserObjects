using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Adaptor to represent a sequence as an indexable buffer like an array or list.
    /// </summary>
    public class SequenceBuffer
    {
        private readonly ISequence<char> _input;
        private readonly List<char> _buffer;
        private readonly int _offset;

        public SequenceBuffer(ISequence<char> input)
        {
            Assert.ArgumentNotNull(input, nameof(input));
            _input = input;
            _buffer = new List<char>();
            _offset = 0;
        }

        private SequenceBuffer(ISequence<char> input, List<char> buffer, int offset)
        {
            _input = input;
            _buffer = buffer;
            _offset = offset;
        }

        public SequenceBuffer CopyFrom(int i) => new SequenceBuffer(_input, _buffer, i);

        public string Capture(int i)
        {
            var chars = _buffer.Skip(_offset).Take(i).ToArray();
            var str = new string(chars);
            for (int j = _buffer.Count - 1; j >= i; j--)
                _input.PutBack(_buffer[j]);
            return str;
        }

        public char this[int index]
        {
            get
            {
                var realIndex = _offset + index;
                FillUntil(realIndex);
                if (realIndex >= _buffer.Count)
                    return '\0';
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
