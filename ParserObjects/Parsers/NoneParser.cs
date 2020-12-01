using ParserObjects.Utility;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser which wraps an inner parser, and guarantees that no input is consumed.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class NoneParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _inner;

        public NoneParser(IParser<TInput, TOutput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var cp = state.Input.Checkpoint();
            var result = _inner.Parse(state);
            if (result.Success)
                cp.Rewind();
            return result;
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public override string ToString() => ParserDefaultStringifier.ToString(this);
    }

    /// <summary>
    /// Parser which wraps an inner parser, and guarantees that no input is consumed.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class NoneParser<TInput> : IParser<TInput>
    {
        private readonly IParser<TInput> _inner;

        public NoneParser(IParser<TInput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }

        public IResult Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var cp = state.Input.Checkpoint();
            var result = _inner.Parse(state);
            if (result.Success)
                cp.Rewind();
            return result;
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public override string ToString() => ParserDefaultStringifier.ToString(this);
    }
}
