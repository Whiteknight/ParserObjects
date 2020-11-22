using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Matches any input item that isn't the end of input. Consumes exactly one input item and
    /// returns it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AnyParser<T> : IParser<T, T>
    {
        // This is functionally equivalent to MatchPredicateParser(x => true) but a little faster
        // and convenient to have for testing

        public string Name { get; set; }

        public IResult<T> Parse(ParseState<T> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            if (state.Input.IsAtEnd)
                return state.Fail(this, "Expected any but found End");
            var location = state.Input.CurrentLocation;
            var next = state.Input.GetNext();
            return state.Success(this, next, location);
        }

        IResult IParser<T>.Parse(ParseState<T> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}