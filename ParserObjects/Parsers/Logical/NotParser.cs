using System.Collections.Generic;

namespace ParserObjects.Parsers.Logical
{
    public class NotParser<TInput> : IParser<TInput, bool>
    {
        private readonly IParser<TInput, bool> _p1;

        public NotParser(IParser<TInput, bool> p1)
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

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IParseResult<bool> Parse(ISequence<TInput> t)
        {
            // If p1 fails, return failure
            // Otherwise return success with the inverse value
            var result1 = _p1.Parse(t);
            if (!result1.Success)
                return result1;
            return new SuccessResult<bool>(!result1.Value, result1.Location);
        }

        public IParser Accept(IParserVisitor visitor) => (visitor as ILogicalVisitorDispatcher)?.VisitNot(this);
    }
}