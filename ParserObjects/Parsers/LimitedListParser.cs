using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Executes an inner parser repeatedly, until it fails. All values are returned as a list.
    /// Expects a number of matches between minimum and maximum values, inclusive.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class LimitedListParser<TInput, TOutput> : IParser<TInput, IReadOnlyList<TOutput>>
    {
        private readonly IParser<TInput, TOutput> _parser;

        public LimitedListParser(IParser<TInput, TOutput> parser, int minimum, int? maximum)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));

            Minimum = minimum < 0 ? 0 : minimum;
            Maximum = maximum;
            if (Maximum.HasValue && Maximum < Minimum)
                Maximum = Minimum;
            
            _parser = parser;
        }

        public string Name { get; set; }
        public int Minimum { get; }
        public int? Maximum { get; }

        public IResult<IReadOnlyList<TOutput>> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var checkpoint = state.Input.Checkpoint();
            var location = state.Input.CurrentLocation;
            var items = new List<TOutput>();

            while (Maximum == null || items.Count < Maximum)
            {
                var result = _parser.Parse(state);
                if (!result.Success)
                    break;
                items.Add(result.Value);
            }

            if (Minimum > 0 && items.Count < Minimum)
            {
                checkpoint.Rewind();
                return state.Fail(this, $"Expected at least {Minimum} items but only found {items.Count}", location);
            }

            return state.Success(this, items, location);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}