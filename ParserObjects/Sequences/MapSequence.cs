using System;
using System.Diagnostics;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// A sequence decorator which takes items from the input sequence and transforms them. Notice
    /// that when using MapSequence, you should not directly access the underlying sequence
    /// anymore. Data may be lost, because items put back to the MapSequence cannot be un-mapped
    /// and put back to the underlying sequence.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class MapSequence<TInput, TOutput> : ISequence<TOutput>
    {
        private readonly ISequence<TInput> _inputs;
        private readonly Func<TInput, TOutput> _map;

        private Node _current;

        public MapSequence(ISequence<TInput> inputs, Func<TInput, TOutput> map)
        {
            Assert.ArgumentNotNull(inputs, nameof(inputs));
            Assert.ArgumentNotNull(map, nameof(map));
            _inputs = inputs;
            _map = map;

            // _current is the value that will be read in the next call to .GetNext().
            // _current.Next is the value that will be read when we advance or call .Peek().

            // Read ahead the first value. If the sequence starts empty, this will be the end
            // sentinel
            var startLocation = _inputs.CurrentLocation;
            var isAtEndToStart = _inputs.IsAtEnd;
            var firstValue = _inputs.GetNext();
            var firstOutput = _map(firstValue);
            _current = new Node(firstOutput, startLocation, 0, isAtEndToStart);

            // If the sequence had one element in it, read ahead and get the end sentinel now,
            // because later calls won't read the .IsAtEnd flag until after the _inputs.GetNext()
            if (!isAtEndToStart && _inputs.IsAtEnd)
            {
                var endSentinelLocation = _inputs.CurrentLocation;
                var endSentinelValue = _inputs.GetNext();
                var endSentinelOutput = _map(endSentinelValue);
                _current.Next = new Node(endSentinelOutput, endSentinelLocation, 1, true);
            }
        }

        // At each Node, .Value is the value that will be returned from .GetNext(), while
        // .Location and .Consumed are the values that should be returned from .CurrentLocation
        // and .Consumed right now.

        private class Node
        {
            public Node(TOutput value, Location location, int consumed, bool end)
            {
                Value = value;
                Location = location;
                Consumed = consumed;
                End = end;
            }

            public TOutput Value { get; }
            public Node? Next { get; set; }
            public Location Location { get; }
            public int Consumed { get; }
            public bool End { get; }
        }

        public TOutput GetNext() => GetNext(true);

        public TOutput Peek() => GetNext(false);

        public Location CurrentLocation => _current.Location;

        public bool IsAtEnd => _current.Next == null && _inputs.IsAtEnd;

        public int Consumed => _current.Consumed;

        public ISequenceCheckpoint Checkpoint() => new SequenceCheckpoint(this, _current);

        private TOutput GetNext(bool advance)
        {
            // If this node is the end node, just return it. We don't advance or do anything
            // beyond this.
            if (_current.End)
                return _current.Value;

            var requestedOutput = _current.Value;

            if (!advance)
                return requestedOutput;

            if (_current.Next != null)
            {
                _current = _current.Next;
                return requestedOutput;
            }

            // Read ahead to queue up the next value. Also we check if we set the IsAtEnd flag, and
            // queue up the end sentinel also if so.

            Debug.Assert(!_inputs.IsAtEnd, "End sentinels should be queued ahead of time");
            var startLocation = _inputs.CurrentLocation;
            var value = _inputs.GetNext();
            var output = _map(value);

            var nextNode = new Node(output, startLocation, _current.Consumed + 1, false);

            // If this read has set the .IsAtEnd flag, queue up the end sentinel
            if (_inputs.IsAtEnd)
            {
                // We're at end. Get one more value so we can see what the end sentinel is. Map
                // the end sentinel, add a new End node to the chain so we know not to advance
                // beyond it.
                var endSentinelLocation = _inputs.CurrentLocation;
                var endSentinelValue = _inputs.GetNext();
                var endSentinelOutput = _map(endSentinelValue);
                nextNode.Next = new Node(endSentinelOutput, endSentinelLocation, _current.Consumed + 1, true);
            }

            _current.Next = nextNode;
            _current = nextNode;
            return requestedOutput;
        }

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly MapSequence<TInput, TOutput> _s;
            private readonly Node _node;

            public SequenceCheckpoint(MapSequence<TInput, TOutput> s, Node node)
            {
                _s = s;
                _node = node;
            }

            public int Consumed => _node.Consumed;

            public Location Location => _node.Location;

            public void Rewind() => _s.Rewind(_node);
        }

        private void Rewind(Node node)
        {
            _current = node;
        }
    }
}
