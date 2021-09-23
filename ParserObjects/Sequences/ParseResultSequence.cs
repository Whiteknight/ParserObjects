using System;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// An adaptor to change output values from an IParser into an ISequence of results.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class ParseResultSequence<TInput, TOutput> : ISequence<IResult<TOutput>>
    {
        private readonly ISequence<TInput> _input;
        private readonly ParseState<TInput> _state;
        private readonly IParser<TInput, TOutput> _parser;
        private readonly SequenceStatistics _stats;

        private Node _current;

        public ParseResultSequence(ISequence<TInput> input, IParser<TInput, TOutput> parser, Action<string> log)
        {
            Assert.ArgumentNotNull(input, nameof(input));
            Assert.ArgumentNotNull(parser, nameof(parser));
            _state = new ParseState<TInput>(input, log);
            _input = input;
            _parser = parser;
            _stats = new SequenceStatistics();

            var startLocation = _input.CurrentLocation;
            var isAtEndToStart = _input.IsAtEnd;
            var firstResult = _parser.Parse(_state);
            _current = new Node(firstResult, startLocation, 0, isAtEndToStart);

            if (!isAtEndToStart && _input.IsAtEnd)
            {
                var endSentinelResult = _parser.Parse(_state);
                _current.Next = new Node(endSentinelResult, endSentinelResult.Location, 1, true);
            }
        }

        private class Node
        {
            public Node(IResult<TOutput> value, Location location, int consumed, bool isAtEnd)
            {
                Value = value;
                Location = location;
                Consumed = consumed;
                IsAtEnd = isAtEnd;
            }

            public IResult<TOutput> Value { get; }
            public Location Location { get; }
            public int Consumed { get; }
            public bool IsAtEnd { get; }

            public Node? Next { get; set; }
        }

        public IResult<TOutput> GetNext() => GetNext(true);

        public IResult<TOutput> Peek() => GetNext(false);

        public Location CurrentLocation => _current.Location;

        public bool IsAtEnd => _current.IsAtEnd;

        public int Consumed => _current.Consumed;

        public ISequenceCheckpoint Checkpoint()
        {
            _stats.CheckpointsCreated++;
            return new SequenceCheckpoint(this, _current);
        }

        private IResult<TOutput> GetNext(bool advance)
        {
            if (_current.IsAtEnd)
                return _current.Value;

            var requestedResult = _current.Value;
            if (!advance)
            {
                _stats.ItemsPeeked++;
                return requestedResult;
            }

            if (_current.Next != null)
            {
                _stats.ItemsRead++;
                _current = _current.Next;
                return requestedResult;
            }

            var nextResult = _parser.Parse(_state);
            _current.Next = new Node(nextResult, nextResult.Location, _current.Consumed + 1, false);
            if (_input.IsAtEnd)
            {
                var endSentinelResult = _parser.Parse(_state);
                _current.Next.Next = new Node(endSentinelResult, endSentinelResult.Location, _current.Consumed + 1, true);
            }

            _stats.ItemsRead++;
            _current = _current.Next;

            return requestedResult;
        }

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly ParseResultSequence<TInput, TOutput> _s;
            private readonly Node _node;

            public SequenceCheckpoint(ParseResultSequence<TInput, TOutput> s, Node node)
            {
                _s = s;
                _node = node;
            }

            public int Consumed => _node.Consumed;

            public Location Location => _node.Location;

            public void Rewind()
            {
                _s.Rewind(_node);
            }
        }

        private void Rewind(Node node)
        {
            _stats.Rewinds++;
            _current = node;
        }

        public ISequenceStatistics GetStatistics() => _stats.Snapshot();
    }
}
