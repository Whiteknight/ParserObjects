using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence decorator which takes items from the input sequence and transforms them
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class MapSequence<TInput, TOutput> : ISequence<TOutput>
    {
        private readonly ISequence<TInput> _inputs;
        private readonly Func<TInput, TOutput> _map;
        private readonly Stack<TOutput> _putbacks;
        private readonly AlwaysFullRingBuffer<Location> _oldLocations;

        private Node _current;

        public MapSequence(ISequence<TInput> inputs, Func<TInput, TOutput> map)
        {
            Assert.ArgumentNotNull(inputs, nameof(inputs));
            Assert.ArgumentNotNull(map, nameof(map));
            _inputs = inputs;
            _map = map;
            _putbacks = new Stack<TOutput>();
            _oldLocations = new AlwaysFullRingBuffer<Location>(5, new Location(null, 0, 0));
            _current = new Node { Value = default, Next = null };
        }

        private class Node
        {
            public TOutput Value { get; set; }
            public Node Next { get; set; }
        }

        public void PutBack(TOutput value)
        {
            _putbacks.Push(value);
            _oldLocations.MoveBack();
        }

        public TOutput GetNext()
        {
            if (_putbacks.Count > 0)
            {
                _oldLocations.MoveForward();
                return _putbacks.Pop();
            }
            if (_current.Next != null)
            {
                _current = _current.Next;
                return _current.Value;
            }

            var value = _inputs.GetNext();
            var output = _map(value);

            var node = new Node { Value = output, Next = null };
            _current.Next = node;
            _current = node;

            _oldLocations.Add(_inputs.CurrentLocation);

            return output;
        }

        public TOutput Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            var next = GetNext();
            PutBack(next);
            return next;
        }

        public Location CurrentLocation => _oldLocations.GetCurrent() ?? _inputs.CurrentLocation;

        public bool IsAtEnd => _putbacks.Count == 0 && _inputs.IsAtEnd;

        public ISequenceCheckpoint Checkpoint() => new SequenceCheckpoint(this, _current, _putbacks.ToArray());

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly MapSequence<TInput, TOutput> _s;
            private readonly Node _node;
            private readonly TOutput[] _putbacks;

            public SequenceCheckpoint(MapSequence<TInput, TOutput> s, Node node, TOutput[] putbacks)
            {
                _s = s;
                _node = node;
                _putbacks = putbacks;
            }

            public void Rewind() => _s.Rewind(_node, _putbacks);
        }

        private void Rewind(Node node, TOutput[] putbacks)
        {
            _current = node;
            _putbacks.Clear();
            for (int i = putbacks.Length - 1; i >= 0; i--)
                _putbacks.Push(putbacks[i]);
        }
    }
}