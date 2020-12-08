using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Negative lookahead parser. Tests the input to see if the inner parser matches. Return
    /// success if the parser does not match, fail otherwise. Consumes no input.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class NegativeLookaheadParser<TInput> : IParser<TInput>
    {
        private readonly IParser<TInput> _inner;

        public NegativeLookaheadParser(IParser<TInput> inner)
        {
            Assert.ArgumentNotNull(inner, nameof(inner));
            _inner = inner;
        }

        public string Name { get; set; }

        public IResult Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var checkpoint = state.Input.Checkpoint();
            var result = _inner.Parse(state);
            if (result.Success)
            {
                checkpoint.Rewind();
                return state.Fail(this, "Lookahead pattern existed but was not supposed to");
            }

            return state.Success(this, null, 0);
        }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
