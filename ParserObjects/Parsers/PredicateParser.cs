using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class PredicateParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<TInput, bool> _predicate;
        private readonly Func<TInput, TOutput> _produce;

        public PredicateParser(Func<TInput, bool> predicate, Func<TInput, TOutput> produce)
        {
            _predicate = predicate;
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var location = t.CurrentLocation;
            if (t.IsAtEnd)
                return new FailResult<TOutput>(location);
            var next = t.Peek();
            if (!_predicate(next))
                return new FailResult<TOutput>(location);
            return new SuccessResult<TOutput>(_produce(t.GetNext()), location);
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t)
        {
            var location = t.CurrentLocation;
            var next = t.Peek();
            if (!_predicate(next))
                return new FailResult<object>(location);
            return new SuccessResult<object>(_produce(t.GetNext()), location);
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}