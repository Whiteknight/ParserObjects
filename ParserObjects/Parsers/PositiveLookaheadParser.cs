using System.Collections.Generic;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Does a lookahead to see if there is a match. Returns a success or failure result, but does not
    /// consume any actual input
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class PositiveLookaheadParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _inner;

        public PositiveLookaheadParser(IParser<TInput, TOutput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TInput, TOutput> typed)
                return new PositiveLookaheadParser<TInput, TOutput>(typed);
            return this;
        }

        public IResult<TOutput> Parse(ISequence<TInput> t)
        {
            var window = new WindowSequence<TInput>(t);
            var result = _inner.Parse(window);
            window.Rewind();
            return result;
        }

        public IResult<object> ParseUntyped(ISequence<TInput> t)
        {
            var window = new WindowSequence<TInput>(t);
            var result = _inner.ParseUntyped(window);
            window.Rewind();
            return result;
        }
    }
}