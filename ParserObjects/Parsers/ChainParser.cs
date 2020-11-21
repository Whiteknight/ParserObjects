using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class ChainParser<TInput, TMiddle, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _inner;
        private readonly Func<TMiddle, IParser<TInput, TOutput>> _getParser;

        public ChainParser(IParser<TInput, TMiddle> inner, Func<TMiddle, IParser<TInput, TOutput>> getParser)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _inner = inner;
            _getParser = getParser;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var initial = _inner.Parse(state);
            if (!initial.Success)
                return initial.Transform(v => default(TOutput));

            var nextParser = _getParser(initial.Value);
            if (nextParser == null)
            {
                checkpoint.Rewind();
                return state.Fail(this, "Get parser callback returned null");
            }

            return nextParser.Parse(state);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _inner && replace is IParser<TInput, TMiddle> replaceTyped)
                return new ChainParser<TInput, TMiddle, TOutput>(replaceTyped, _getParser);
            return this;
        }
    }
}
