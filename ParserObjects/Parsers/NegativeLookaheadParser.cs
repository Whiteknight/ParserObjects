using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Negative lookahead parser. Tests the input to see if the inner parser matches. Return 
    /// success if the parser does not match, fail otherwise. Consumes no input.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class NegativeLookaheadParser<TInput> : IParser<TInput, object>
    {
        private readonly IParser<TInput> _inner;

        public NegativeLookaheadParser(IParser<TInput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TInput> typed)
                return new NegativeLookaheadParser<TInput>(typed);
            return this;
        }

        public IResult<object> Parse(ParseState<TInput> t) => ParseUntyped(t);

        public IResult<object> ParseUntyped(ParseState<TInput> t)
        {
            var checkpoint = t.Input.Checkpoint();
            var result = _inner.ParseUntyped(t);
            checkpoint.Rewind();
            return Result.New<object>(!result.Success, null, result.Location);
        }
    }
}