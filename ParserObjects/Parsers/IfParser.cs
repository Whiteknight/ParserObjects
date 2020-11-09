using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Attempts to match a predicate condition and, on success, invokes a parser.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class IfParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput> _predicate;
        private readonly IParser<TInput, TOutput> _inner;

        public IfParser(IParser<TInput> predicate, IParser<TInput, TOutput> inner)
        {
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            Assert.ArgumentNotNull(inner, nameof(inner));
            _predicate = predicate;
            _inner = inner;
        }

        public IResult<TOutput> Parse(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var predicatePassed = TestPredicate(t);
            if (predicatePassed)
                return _inner.Parse(t);
            return Result.Fail<TOutput>(t.CurrentLocation);
        }

        private bool TestPredicate(ISequence<TInput> t)
        {
            var window = t.Window();
            var result = _predicate.ParseUntyped(window);
            window.Rewind();
            bool predicatePassed = result.Success;
            return predicatePassed;
        }

        public IResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _predicate, _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _predicate && replace is IParser<TInput> predicate)
                return new IfParser<TInput, TOutput>(predicate, _inner);
            if (find == _inner && replace is IParser<TInput, TOutput> inner)
                return new IfParser<TInput, TOutput>(_predicate, inner);
            return this;
        }
    }
}