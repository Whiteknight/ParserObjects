using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Tests the next input and returns it if it matches.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PredicateParser<T> : IParser<T, T>
    {
        private readonly Func<T, bool> _predicate;

        public PredicateParser(Func<T, bool> predicate)
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

        public IParseResult<object> ParseUntyped(ISequence<T> t)
        {
            var location = t.CurrentLocation;
            if (t.IsAtEnd)
                return new FailResult<object>(location);
            var next = t.Peek();
            if (!_predicate(next))
                return new FailResult<object>(location);
            return new SuccessResult<object>(t.GetNext(), location);
        }

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