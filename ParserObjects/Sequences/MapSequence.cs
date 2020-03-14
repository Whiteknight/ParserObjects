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

        public MapSequence(ISequence<TInput> inputs, Func<TInput, TOutput> map)
        {
            Assert.ArgumentNotNull(inputs, nameof(inputs));
            Assert.ArgumentNotNull(map, nameof(map));
            _inputs = inputs;
            _map = map;
            _putbacks = new Stack<TOutput>();
            _oldLocations = new AlwaysFullRingBuffer<Location>(5, new Location(null, 0, 0));
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

            if (_inputs.IsAtEnd)
                return default;

            var next = _inputs.GetNext();
            _oldLocations.Add(_inputs.CurrentLocation);
            var output = _map(next);
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
    }
}