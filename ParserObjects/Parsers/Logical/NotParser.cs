using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers.Logical
{
    public class NotParser<TInput> : IParser<TInput, object>
    {
        private readonly IParser<TInput> _inner;

        public NotParser(IParser<TInput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }
        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _inner && replace is IParser<TInput, bool> typed1)
                return new NotParser<TInput>(typed1);
            return this;
        }

        public IParseResult<object> Parse(ISequence<TInput> t) => ParseUntyped(t);

        public IParseResult<object> ParseUntyped(ISequence<TInput> t)
        {
            var result1 = _inner.ParseUntyped(t);
            if (result1.Success)
                return new FailResult<object>(t.CurrentLocation);
            return new SuccessResult<object>(null, result1.Location);
        }
    }
}