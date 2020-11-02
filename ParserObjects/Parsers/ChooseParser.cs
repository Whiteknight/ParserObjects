using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class ChooseParser<TInput, TMiddle, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _inner;
        private readonly Func<TMiddle, IParser<TInput, TOutput>> _getParser;

        public ChooseParser(IParser<TInput, TMiddle> inner, Func<TMiddle, IParser<TInput, TOutput>> getParser)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _inner = inner;
            _getParser = getParser;
        }

        public string Name { get; set; }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var window = t.Window();
            var middle = _inner.Parse(window);
            if (!middle.Success)
                return new FailResult<TOutput>(t.CurrentLocation);
            window.Rewind();

            var nextParser = _getParser(middle.Value);
            if (nextParser == null)
                return new FailResult<TOutput>(t.CurrentLocation);

            return nextParser.Parse(t);
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _inner && replace is IParser<TInput, TMiddle> replaceTyped)
                return new ChooseParser<TInput, TMiddle, TOutput>(replaceTyped, _getParser);
            return this;
        }
    }
}
