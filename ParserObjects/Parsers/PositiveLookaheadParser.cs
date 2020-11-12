using System.Collections.Generic;
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

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            var checkpoint = t.Input.Checkpoint();
            var result = _inner.Parse(t);
            if (result.Success)
                checkpoint.Rewind();
            return result;
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t)
        {
            var checkpoint = t.Input.Checkpoint();
            var result = _inner.ParseUntyped(t);
            if (result.Success)
                checkpoint.Rewind();
            return result;
        }
    }
}