using System;
using System.Collections.Generic;

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

        public MapSequence(ISequence<TInput> inputs, Func<TInput, TOutput> map)
        {
            _inputs = inputs;
            _map = map;
            _putbacks = new Stack<TOutput>();
        }

        public void PutBack(TOutput value)
        {
            _putbacks.Push(value);
        }

        public TOutput GetNext()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Pop();
            var next = _inputs.GetNext();
            var output = _map(next);
            return output;
        }

        public TOutput Peek()
        {
            var next = GetNext();
            PutBack(next);
            return next;
        }

        // TODO: Is there any generic way we can account for _putbacks?
        public Location CurrentLocation => _inputs.CurrentLocation;

        public bool IsAtEnd => _putbacks.Count == 0 && _inputs.IsAtEnd;
    }
}