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

        public IParseResult<IReadOnlyList<T>> Parse(ISequence<T> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var location = t.CurrentLocation;

            // Handle a few small cases where we don't want to allocate a buffer
            if (Pattern.Count == 0)
                return new SuccessResult<IReadOnlyList<T>>(new T[0], location);
            if (Pattern.Count == 1)
                return t.Peek().Equals(Pattern[0]) ? new SuccessResult<IReadOnlyList<T>>(new[] { t.GetNext() }, location) : (IParseResult<IReadOnlyList<T>>)new FailResult<IReadOnlyList<T>>(location);

            var buffer = new T[Pattern.Count];
            for (var i = 0; i < Pattern.Count; i++)
            {
                var c = t.GetNext();
                buffer[i] = c;
                if (c.Equals(Pattern[i]))
                    continue;

                for (; i >= 0; i--)
                    t.PutBack(buffer[i]);
                return new FailResult<IReadOnlyList<T>>();
            }

            return new SuccessResult<IReadOnlyList<T>>(buffer, location);
        }

        public IParseResult<object> ParseUntyped(ISequence<T> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}