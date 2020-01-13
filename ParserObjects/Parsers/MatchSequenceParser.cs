using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Given a literal sequence of values, pull values off the input sequence to match. If the entire
    /// series matches, return it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MatchSequenceParser<T> : IParser<T, IReadOnlyList<T>>
    {
        private readonly T[] _find;

        public MatchSequenceParser(IEnumerable<T> find)
        {
            _find = find.ToArray();
        }

        public IParseResult<IReadOnlyList<T>> Parse(ISequence<T> t)
        {
            var location = t.CurrentLocation;

            // Handle a few small cases where we don't want to allocate a buffer
            if (t.IsAtEnd && _find.Length > 0)
                return new FailResult<IReadOnlyList<T>>(location);
            if (_find.Length == 0)
                return new SuccessResult<IReadOnlyList<T>>(new T[0], location);
            if (_find.Length == 1)
                return t.Peek().Equals(_find[0]) ? new SuccessResult<IReadOnlyList<T>>(new[] { t.GetNext() }, location) : (IParseResult<IReadOnlyList<T>>)new FailResult<IReadOnlyList<T>>(location);

            var buffer = new T[_find.Length];
            for (var i = 0; i < _find.Length; i++)
            {
                var c = t.GetNext();
                buffer[i] = c;
                if (c.Equals(_find[i]))
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