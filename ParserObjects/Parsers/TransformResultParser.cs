using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Transforms the raw result object, including the result type, result value, success flag or
    /// error message.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput1"></typeparam>
    /// <typeparam name="TOutput2"></typeparam>
    public static class TransformResult<TInput, TOutput1, TOutput2>
    {
        public delegate IResult<TOutput2> Function(ParseState<TInput> state, IResult<TOutput1> result);

        public class Parser : IParser<TInput, TOutput2>
        {
            private readonly IParser<TInput, TOutput1> _inner;
            private readonly Function _transform;

            public Parser(IParser<TInput, TOutput1> inner, Function transform)
            {
                Assert.ArgumentNotNull(inner, nameof(inner));
                Assert.ArgumentNotNull(transform, nameof(transform));
                _inner = inner;
                _transform = transform;
            }

            public string Name { get; set; }

            public IResult<TOutput2> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var result = _inner.Parse(state);
                return _transform(state, result);
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
