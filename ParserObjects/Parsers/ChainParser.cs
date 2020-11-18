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

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));

            var checkpoint = t.Input.Checkpoint();
            var initial = _inner.Parse(t);
            if (!initial.Success)
                return initial.Transform(v => default(TOutput));

            var nextParser = _getParser(initial.Value);
            if (nextParser == null)
            {
                checkpoint.Rewind();
                return t.Fail(this, "Get parser callback returned null");
            }

            return nextParser.Parse(t);
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _inner && replace is IParser<TInput, TMiddle> replaceTyped)
                return new ChainParser<TInput, TMiddle, TOutput>(replaceTyped, _getParser);
            return this;
        }
    }
}
