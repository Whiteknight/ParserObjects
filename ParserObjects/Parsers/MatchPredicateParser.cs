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

        public IResult<T> Parse(ISequence<T> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var location = t.CurrentLocation;
            if (t.IsAtEnd)
                return Result.Fail<T>(location);
            var next = t.Peek();
            if (!_predicate(next))
                return Result.Fail<T>(location);
            return Result.Success(t.GetNext(), location);
        }

        public IResult<object> ParseUntyped(ISequence<T> t) => Parse(t).Untype();

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