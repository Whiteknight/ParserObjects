using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parses a list of productions with no explicit separator. Continues as long as the parser
    /// succeeds. Terminates and returns a ListNode when the parser fails. May return 0 items.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class ListParser<TInput, TItem, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TItem> _parser;
        private readonly Func<IReadOnlyList<TItem>, TOutput> _produce;

        public ListParser(IParser<TInput, TItem> parser, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne)
        {
            _parser = parser;
            _produce = produce;
            AtLeastOne = atLeastOne;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var location = t.CurrentLocation;
            var items = new List<TItem>();
            while (true)
            {
                var result = _parser.Parse(t);
                if (!result.Success)
                    break;
                items.Add(result.Value);
            }

            if (AtLeastOne && items.Count == 0)
                return new FailResult<TOutput>(location);
            return new SuccessResult<TOutput>(_produce(items), location);
        }

        public bool AtLeastOne { get; }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parser == find && replace is IParser<TInput, TItem> realReplace)
                return new ListParser<TInput, TItem, TOutput>(realReplace, _produce, AtLeastOne);
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}