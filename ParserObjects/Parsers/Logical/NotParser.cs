using System.Collections.Generic;

namespace ParserObjects.Parsers.Logical
{
    public class NotParser<TInput> : IParser<TInput, object>
    {
        private readonly IParser<TInput> _p1;

        public NotParser(IParser<TInput> p1)
        {
            _p1 = p1;
        }

        public string Name { get; set; }
        public IEnumerable<IParser> GetChildren() => new[] { _p1 };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _p1 && replace is IParser<TInput, bool> typed1)
                return new NotParser<TInput>(typed1);
            return this;
        }

        public IParseResult<object> Parse(ISequence<TInput> t) => ParseUntyped(t);

        public IParseResult<object> ParseUntyped(ISequence<TInput> t)
        {
            var result1 = _p1.ParseUntyped(t);
            if (result1.Success)
                return new FailResult<object>(t.CurrentLocation);
            return new SuccessResult<object>(null, result1.Location);
        }
    }
}