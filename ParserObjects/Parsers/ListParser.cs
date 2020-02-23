using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parses an enumerable of results so long as the parser continues to match. Returns an enumerable
    /// of the parsed results.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class ListParser<TInput, TOutput> : IParser<TInput, IEnumerable<TOutput>>
    {
        private readonly IParser<TInput, TOutput> _parser;

        public ListParser(IParser<TInput, TOutput> parser, bool atLeastOne)
        {
            _parser = parser;
            AtLeastOne = atLeastOne;
        }

        public IParseResult<IEnumerable<TOutput>> Parse(ISequence<TInput> t)
        {
            var location = t.CurrentLocation;
            var items = new List<TOutput>();
            while (true)
            {
                var result = _parser.Parse(t);
                if (!result.Success)
                    break;
                items.Add(result.Value);
            }

            if (AtLeastOne && items.Count == 0)
                return new FailResult<IEnumerable<TOutput>>(location);
            return new SuccessResult<IEnumerable<TOutput>>(items, location);
        }

        public bool AtLeastOne { get; }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parser == find && replace is IParser<TInput, TOutput> realReplace)
                return new ListParser<TInput, TOutput>(realReplace, AtLeastOne);
            return this;
        }

        public IParser Accept(IParserVisitor visitor) => (visitor as ICoreVisitorDispatcher)?.VisitList(this) ?? this;

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}