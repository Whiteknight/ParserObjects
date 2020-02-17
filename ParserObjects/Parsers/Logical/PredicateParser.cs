using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers.Logical
{
    public class PredicateParser<TInput> : IParser<TInput, bool>
    {
        private readonly Func<ISequence<TInput>, bool> _predicate;

        public PredicateParser(Func<ISequence<TInput>, bool> predicate)
        {
            _predicate = predicate;
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IParseResult<bool> Parse(ISequence<TInput> t) 
            => new SuccessResult<bool>(_predicate(t), t.CurrentLocation);
    }
}