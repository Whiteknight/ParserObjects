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

        public Result<object> Parse(ParseState<TInput> t) => ParseUntyped(t);

        public Result<object> ParseUntyped(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var checkpoint = t.Input.Checkpoint();
            var (success, _) = _inner.ParseUntyped(t);
            checkpoint.Rewind();
            if (success)
                return t.Fail(this, "Lookahead pattern existed but was not supposed to");
            return t.Success(this, null);
        }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TInput> typed)
                return new NegativeLookaheadParser<TInput>(typed);
            return this;
        }
    }
}