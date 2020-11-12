using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Returns the next input item if it satisfies a predicate, failure otherwise.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MatchPredicateParser<T> : IParser<T, T>
    {
        private readonly Func<T, bool> _predicate;

        public MatchPredicateParser(Func<T, bool> predicate)
        {
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            _predicate = predicate;
        }

        public IResult<T> Parse(ParseState<T> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var location = t.Input.CurrentLocation;
            if (t.Input.IsAtEnd)
                return t.Fail<T>();
            var next = t.Input.Peek();
            if (!_predicate(next))
                return t.Fail<T>();
            return t.Success(t.Input.GetNext(), location);
        }

        public IResult<object> ParseUntyped(ParseState<T> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}