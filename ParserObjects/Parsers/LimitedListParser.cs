using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class LimitedListParser<TInput, TOutput> : IParser<TInput, IReadOnlyList<TOutput>>
    {
        private readonly IParser<TInput, TOutput> _parser;

        public LimitedListParser(IParser<TInput, TOutput> parser, int minimum, int? maximum)
        {
            Minimum = minimum < 0 ? 0 : minimum;
            Maximum = maximum;
            if (Maximum.HasValue && Maximum < Minimum)
                Maximum = Minimum;
            Assert.ArgumentNotNull(parser, nameof(parser));
            _parser = parser;
        }

        public string Name { get; set; }
        public int Minimum { get; }
        public int? Maximum { get; }

        public IResult<IReadOnlyList<TOutput>> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));

            var checkpoint = t.Input.Checkpoint();
            var location = t.Input.CurrentLocation;
            var items = new List<TOutput>();

            while (Maximum == null || items.Count < Maximum)
            {
                var result = _parser.Parse(t);
                if (!result.Success)
                    break;
                items.Add(result.Value);
            }

            if (Minimum > 0 && items.Count < Minimum)
            {
                checkpoint.Rewind();
                return t.Fail(this, $"Expected at least {Minimum} items but only found {items.Count}", location);
            }

            return t.Success(this, items, location);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> t) => Parse(t);

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parser == find && replace is IParser<TInput, TOutput> realReplace)
                return new LimitedListParser<TInput, TOutput>(realReplace, Minimum, Maximum);
            return this;
        }

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}