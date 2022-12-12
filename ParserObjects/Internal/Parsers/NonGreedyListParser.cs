using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public static class NonGreedyList<TInput, TMiddle, TOutput>
{
    public class Parser : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _itemParser;
        private readonly Func<IParser<TInput, IReadOnlyList<TMiddle>>, IParser<TInput, TOutput>> _getContinuation;
        private readonly int _minimum;
        private readonly int? _maximum;
        private readonly LeftValue _leftValue;
        private readonly IParser<TInput, TOutput> _rightParser;

        public Parser(IParser<TInput, TMiddle> itemParser, Func<IParser<TInput, IReadOnlyList<TMiddle>>, IParser<TInput, TOutput>> getContinuation, int minimum, int? maximum, string name = "")
        {
            _itemParser = itemParser;
            _getContinuation = getContinuation;
            _minimum = minimum;
            _maximum = maximum;
            Name = name;
            _leftValue = new LeftValue(Name);
            _rightParser = getContinuation(_leftValue);
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _leftValue, _rightParser };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();
            _leftValue.Location = startCp.Location;

            int count = 0;
            while (true)
            {
                var result = _rightParser.Parse(state);
                if (result.Success)
                {
                    if (count >= _minimum)
                        return result;

                    startCp.Rewind();
                    return state.Fail(this, "Could not match minimum number of items");
                }

                var nextItemResult = _itemParser.Parse(state);
                if (!nextItemResult.Success)
                {
                    startCp.Rewind();
                    return state.Fail(this, "Could not match");
                }

                count++;
                if (_maximum.HasValue && count > _maximum.Value)
                {
                    startCp.Rewind();
                    return state.Fail(this, "Matched too many items without a continuation");
                }

                _leftValue.Value.Add(nextItemResult.Value);
            }
        }

        public INamed SetName(string name) => new Parser(_itemParser, _getContinuation, _minimum, _maximum, name);

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        private class LeftValue : IParser<TInput, IReadOnlyList<TMiddle>>, IHiddenInternalParser
        {
            public LeftValue(string name)
            {
                Name = name;
                Value = new List<TMiddle>();
            }

            public List<TMiddle> Value { get; }

            public Location Location { get; set; }

            public int Id { get; } = UniqueIntegerGenerator.GetNext();

            public string Name { get; }

            public IResult<IReadOnlyList<TMiddle>> Parse(IParseState<TInput> state) => state.Success(this, Value, 0, Location);

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => state.Success(this, Value!, 0, Location);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);

            public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename the internal value parser");
        }
    }
}
