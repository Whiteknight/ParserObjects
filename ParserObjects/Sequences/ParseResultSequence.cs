using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Sequences
{
    /// <summary>
    /// An adaptor to change output values from an IParser into an ISequence of results
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class ParseResultSequence<TInput, TOutput> : ISequence<IResult<TOutput>>
    {
        private readonly ISequence<TInput> _input;
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Stack<IResult<TOutput>> _putbacks;

        public ParseResultSequence(ISequence<TInput> input, IParser<TInput, TOutput> parser)
        {
            Assert.ArgumentNotNull(input, nameof(input));
            Assert.ArgumentNotNull(parser, nameof(parser));
            _input = input;
            _parser = parser;
            _putbacks = new Stack<IResult<TOutput>>();
        }

        public void PutBack(IResult<TOutput> value)
        {
            _putbacks.Push(value);
        }

        public IResult<TOutput> GetNext()
        {
            if (_putbacks.Count > 0)
                return _putbacks.Pop();
            return _parser.Parse(_input);
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
    }
}
