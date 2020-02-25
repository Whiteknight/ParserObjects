using System;
using System.Collections.Generic;
using System.Linq;

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
            _predicate = predicate;
        }

        public IParseResult<T> Parse(ISequence<T> t)
        {
            var location = t.CurrentLocation;
            if (t.IsAtEnd)
                return new FailResult<T>(location);
            var next = t.Peek();
            if (!_predicate(next))
                return new FailResult<T>(location);
            return new SuccessResult<T>(t.GetNext(), location);
        }

        public IParseResult<object> ParseUntyped(ISequence<T> t) => Parse(t).Untype();

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