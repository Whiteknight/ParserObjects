using System.Collections.Generic;

namespace ParserObjects.Parsers.Logical
{
    public class OrParser<TInput> : IParser<TInput, bool>
    {
        private readonly IParser<TInput, bool> _p1;
        private readonly IParser<TInput, bool> _p2;

        public OrParser(IParser<TInput, bool> p1, IParser<TInput, bool> p2)
        {
            _p1 = p1;
            _p2 = p2;
        }

        public string Name { get; set; }
        public IEnumerable<IParser> GetChildren() => new[] { _p1, _p2 };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _p1 && replace is IParser<TInput, bool> typed1)
                return new OrParser<TInput>(typed1, _p2);
            if (find == _p2 && replace is IParser<TInput, bool> typed2)
                return new OrParser<TInput>(_p1, typed2);
            return this;
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IParseResult<bool> Parse(ISequence<TInput> t)
        {
            // If p1 fails or succeeds(true) return result1, it is identical to what we want to return
            // if p1 succeeds(false) and p2 succeeds, return result2
            // If p1 succeeds(false) but p2 fails, return failure
            var result1 = _p1.Parse(t);
            if (!result1.Success || result1.Value)
                return result1;

            var result2 = _p2.Parse(t);
            if (result2.Success)
                return result2;

            return new FailResult<bool>(result2.Location);
        }
    }
}