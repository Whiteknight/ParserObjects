using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// Wraps an IEnumerable as an ISequence. Makes the items from the enumerable usable in parse
    /// operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ListSequence<T> : ISequence<T>
    {
        private readonly IReadOnlyList<T> _list;
        private readonly T? _endSentinelValue;

        private SequenceStatistics _stats;
        private int _index;

        public ListSequence(IEnumerable<T> enumerable, T? endSentinel)
        {
            Assert.ArgumentNotNull(enumerable, nameof(enumerable));
            _list = enumerable is IReadOnlyList<T> list ? list : enumerable.ToList();
            _endSentinelValue = endSentinel;

            _index = 0;

            _stats = default;
        }

        public ListSequence(IReadOnlyList<T> list, T? endSentinel)
        {
            Assert.ArgumentNotNull(list, nameof(list));
            _list = list;
            _endSentinelValue = endSentinel;

            _index = 0;

            _stats = default;
        }

        private class Node
        {
            public T? Value { get; set; }
            public Node? Next { get; set; }
        }

        public T GetNext()
        {
            if (_index >= _list.Count)
                return _endSentinelValue;

            var value = _list[_index];
            _index++;

            _stats.ItemsRead++;
            return value;
        }

        public T Peek()
        {
            if (_index >= _list.Count)
                return _endSentinelValue;

            return _list[_index];
        }

        public Location CurrentLocation => new Location(string.Empty, 1, _index);

        public bool IsAtEnd => _index >= _list.Count;

        public int Consumed => _index;

        public ISequenceCheckpoint Checkpoint()
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(this, _index);
        }

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly ListSequence<T> _s;
            private readonly int _index;

            public SequenceCheckpoint(ListSequence<T> s, int index)
            {
                _s = s;
                _index = index;
            }

            public int Consumed => _index;

            public Location Location => new Location(string.Empty, 1, _index);

            public void Rewind() => _s.Rewind(_index);
        }

        private void Rewind(int index)
        {
            _stats.Rewinds++;
            _index = index;
        }

        public ISequenceStatistics GetStatistics() => _stats;
    }
}
