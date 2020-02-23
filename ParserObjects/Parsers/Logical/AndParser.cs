using System.Collections.Generic;
using ParserObjects.Sequences;

namespace ParserObjects.Parsers.Logical
{
    public class AndParser<TInput> : IParser<TInput, bool>
    {
        private readonly IParser<TInput, bool> _p1;
        private readonly IParser<TInput, bool> _p2;

        public AndParser(IParser<TInput, bool> p1, IParser<TInput, bool> p2)
        {
            _p1 = p1;
            _p2 = p2;
        }

        public string Name { get; set; }
        public IEnumerable<IParser> GetChildren() => new [] { _p1, _p2 };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _p1 && replace is IParser<TInput, bool> typed1)
                return new AndParser<TInput>(typed1, _p2);
            if (find == _p2 && replace is IParser<TInput, bool> typed2)
                return new AndParser<TInput>(_p1, typed2);
            return this;
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IParseResult<bool> Parse(ISequence<TInput> t)
        {
            // If p1 fails, return failure.
            // if p1 succeeds(false) return success(false)
            // If p1 succeeds(true) but p2 fails, rewind and return failure
            // If p1 succeeds(true) and p2 succeeds(false) return success(false)
            // if p1 succeeds(true) and p2 succeeds(true) return success(true)
            var window = new WindowSequence<TInput>(t);
            var result1 = _p1.Parse(window);
            if (!result1.Success)
                return new FailResult<bool>(result1.Location);
            if (!result1.Value)
                return new SuccessResult<bool>(false, result1.Location);

            var result2 = _p2.Parse(window);
            if (result2.Success)
                return result2;

            // If p1 succeeds but p2 fails, we need to rewind the input that p1 consumed
            window.Rewind();
            return new FailResult<bool>(result2.Location);
        }

        public IParser Accept(IParserVisitor visitor) => (visitor as ILogicalVisitorDispatcher)?.VisitAnd(this);
    }
}
