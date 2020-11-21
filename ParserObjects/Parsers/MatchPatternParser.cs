using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Given a literal sequence of values, pull values off the input sequence to match. If the entire
    /// series matches, return it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MatchPatternParser<T> : IParser<T, IReadOnlyList<T>>
    {
        public IReadOnlyList<T> Pattern { get; }

        public MatchPatternParser(IEnumerable<T> find)
        {
            Assert.ArgumentNotNull(find, nameof(find));
            Pattern = find.ToArray();
        }

        public string Name { get; set; }

        public IResult<IReadOnlyList<T>> Parse(ParseState<T> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var location = state.Input.CurrentLocation;

            // If the pattern is empty, return success.
            if (Pattern.Count == 0)
            {
                state.Log(this, "Pattern has 0 items in it, this is functionally equivalent to Empty() ");
                return state.Success(this, new T[0], location);
            }

            // If the pattern has exactly one item in it, check for equality without a loop 
            // or allocating a buffer
            if (Pattern.Count == 1)
            {
                if (state.Input.Peek().Equals(Pattern[0]))
                    return state.Success(this, new[] { state.Input.GetNext() }, location);
                return state.Fail(this, "Item does not match");
            }

            var checkpoint = state.Input.Checkpoint();
            var buffer = new T[Pattern.Count];
            for (var i = 0; i < Pattern.Count; i++)
            {
                var c = state.Input.GetNext();
                buffer[i] = c;
                if (c.Equals(Pattern[i]))
                    continue;

                checkpoint.Rewind();
                return state.Fail(this, $"Item does not match at position {i}");
            }

            return state.Success(this, buffer, location);
        }

        IResult IParser<T>.Parse(ParseState<T> t) => Parse(t);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}