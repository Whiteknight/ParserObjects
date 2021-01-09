using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class PeekParser<T> : IParser<T, T>
    {
        public PeekParser()
        {
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IResult<T> Parse(IParseState<T> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            if (state.Input.IsAtEnd)
                return state.Fail(this, "Expected any but found End");

            var peek = state.Input.Peek();
            return state.Success(this, peek, 0, state.Input.CurrentLocation);
        }

        IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
