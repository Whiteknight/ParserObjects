using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    public class TransformResultParser<TInput, TOutput1, TOutput2> : IParser<TInput, TOutput2>
    {
        private readonly IParser<TInput, TOutput1> _inner;
        private readonly Func<ParseState<TInput>, IResult<TOutput1>, IResult<TOutput2>> _transform;

        public TransformResultParser(IParser<TInput, TOutput1> inner, Func<ParseState<TInput>, IResult<TOutput1>, IResult<TOutput2>> transform)
        {
            _inner = inner;
            _transform = transform;
        }

        public string Name { get; set; }

        public IResult<TOutput2> Parse(ParseState<TInput> state)
        {
            var result = _inner.Parse(state);
            return _transform(state, result);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> t) => Parse(t);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _inner && replace is IParser<TInput, TOutput1> typedReplace)
                return new TransformResultParser<TInput, TOutput1, TOutput2>(typedReplace, _transform);
            return this;
        }
    }
}
