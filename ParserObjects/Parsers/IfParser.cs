using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Attempts to match a predicate condition and, on success, invokes a parser.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class IfParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, bool> _predicate;
        private readonly IParser<TInput, TOutput> _inner;

        public IfParser(IParser<TInput, bool> predicate, IParser<TInput, TOutput> inner)
        {
            _predicate = predicate;
            _inner = inner;
        }

        public string Name { get; set; }
        public IEnumerable<IParser> GetChildren() => new IParser[] { _predicate, _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _predicate && replace is IParser<TInput, bool> predicate)
                return new IfParser<TInput, TOutput>(predicate, _inner);
            if (find == _inner && replace is IParser<TInput, TOutput> inner)
                return new IfParser<TInput, TOutput>(_predicate, inner);
            return this;
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var result = _predicate.Parse(t);
            return result.Success && result.Value ? _inner.Parse(t) : new FailResult<TOutput>(t.CurrentLocation);
        }
    }
}