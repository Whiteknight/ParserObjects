using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    public class TransformResultParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _inner;
        private readonly Func<IResult<TOutput>, IResult<TOutput>> _transform;

        public TransformResultParser(IParser<TInput, TOutput> inner, Func<IResult<TOutput>, IResult<TOutput>> transform)
        {
            _inner = inner;
            _transform = transform;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            var result = _inner.Parse(t);
            return _transform(result);
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _inner && replace is IParser<TInput, TOutput> typedReplace)
                return new TransformResultParser<TInput, TOutput>(typedReplace, _transform);
            return this;
        }
    }
}
