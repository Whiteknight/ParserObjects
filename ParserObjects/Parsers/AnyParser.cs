using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Matches any input item that isn't the end of input. Consumes exactly one input item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AnyParser<T> : IParser<T, T>
    {
        // This is functionally equivalent to MatchPredicateParser(x => true) but a little faster
        // and convenient to have for testing

        public string Name { get; set; }

        public IResult<T> Parse(ParseState<T> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            if (t.Input.IsAtEnd)
                return t.Fail(this, "Expected any but found End");
            var location = t.Input.CurrentLocation;
            var next = t.Input.GetNext();
            return t.Success(this, next, location);
        }

        IResult IParser<T>.Parse(ParseState<T> t) => Parse(t);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}