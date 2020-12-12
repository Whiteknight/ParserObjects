using System;
using System.Collections.Generic;
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
        private readonly Stack<IResult<TOutput>> _putbacks;

        private int _consumed;

        public ParseResultSequence(ISequence<TInput> input, IParser<TInput, TOutput> parser, Action<string> log)
        {
            Assert.ArgumentNotNull(input, nameof(input));
            Assert.ArgumentNotNull(parser, nameof(parser));
            _state = new ParseState<TInput>(input, log);
            _input = input;
            _parser = parser;
            _putbacks = new Stack<IResult<TOutput>>();
            _consumed = 0;
        }

        public void PutBack(IResult<TOutput> value)
        {
            if (value == null)
                return;
            _putbacks.Push(value);
            _consumed--;
        }

        public IResult<TOutput> GetNext()
        {
            if (_putbacks.Count > 0)
            {
                _consumed++;
                return _putbacks.Pop();
            }

            if (_input.IsAtEnd)
                return _state.Fail(_parser, "The input sequence is at end, the result sequence cannot continue.", _input.CurrentLocation);

            var result = _parser.Parse(_state);
            _consumed++;
            return result;
        }

        public IResult<TOutput> Peek()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Peek();
            var next = GetNext();
            PutBack(next);
            return next;
        }

        public Location CurrentLocation
            => _putbacks.Count > 0 ? _putbacks.Peek().Location : _input.CurrentLocation;

        public bool IsAtEnd => _putbacks.Count == 0 && _input.IsAtEnd;

        public int Consumed => _consumed;

        public ISequenceCheckpoint Checkpoint()
        {
            var innerCheckpoint = _input.Checkpoint();
            return new SequenceCheckpoint(this, innerCheckpoint, _putbacks.ToArray(), _consumed);
        }

        private class SequenceCheckpoint : ISequenceCheckpoint
        {
            private readonly ParseResultSequence<TInput, TOutput> _s;
            private readonly ISequenceCheckpoint _inner;
            private readonly IResult<TOutput>[] _putbacks;
            private readonly int _consumed;

            public SequenceCheckpoint(ParseResultSequence<TInput, TOutput> s, ISequenceCheckpoint inner, IResult<TOutput>[] putbacks, int consumed)
            {
                _s = s;
                _inner = inner;
                _putbacks = putbacks;
                _consumed = consumed;
            }

            public void Rewind()
            {
                _inner.Rewind();
                _s.Rewind(_putbacks, _consumed);
            }
        }

        private void Rewind(IResult<TOutput>[] putbacks, int consumed)
        {
            _putbacks.Clear();
            for (int i = putbacks.Length - 1; i >= 0; i--)
                _putbacks.Push(putbacks[i]);
            _consumed = consumed;
        }
    }
}
