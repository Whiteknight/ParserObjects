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
    public class MatchSequenceParser<T> : IParser<T, IReadOnlyList<T>>
    {
        public IReadOnlyList<T> Pattern { get; }

        public MatchSequenceParser(IEnumerable<T> find)
        {
            Assert.ArgumentNotNull(find, nameof(find));
            Pattern = find.ToArray();
        }

        public IResult<IReadOnlyList<T>> Parse(ParseState<T> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var location = t.Input.CurrentLocation;

            // Handle a few small cases where we don't want to allocate a buffer
            if (Pattern.Count == 0)
                return t.Success<IReadOnlyList<T>>(new T[0], location);
            if (Pattern.Count == 1)
                return t.Input.Peek().Equals(Pattern[0]) ? t.Success<IReadOnlyList<T>>(new[] { t.Input.GetNext() }, location) : t.Fail<IReadOnlyList<T>>();

            var checkpoint = t.Input.Checkpoint();
            var buffer = new T[Pattern.Count];
            for (var i = 0; i < Pattern.Count; i++)
            {
                var c = t.Input.GetNext();
                buffer[i] = c;
                if (c.Equals(Pattern[i]))
                    continue;

                checkpoint.Rewind();
                return t.Fail<IReadOnlyList<T>>();
            }

            return t.Success<IReadOnlyList<T>>(buffer, location);
        }

        public IResult<object> ParseUntyped(ParseState<T> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}