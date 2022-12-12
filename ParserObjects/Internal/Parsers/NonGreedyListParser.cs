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
        private readonly IParser<TInput> _separator;
        private readonly Func<IParser<TInput, IReadOnlyList<TMiddle>>, IParser<TInput, TOutput>> _getContinuation;
        private readonly int _minimum;
        private readonly int? _maximum;
        private readonly LeftValue _leftValue;
        private readonly IParser<TInput, TOutput> _rightParser;

        public Parser(IParser<TInput, TMiddle> itemParser, IParser<TInput> separator, Func<IParser<TInput, IReadOnlyList<TMiddle>>, IParser<TInput, TOutput>> getContinuation, int minimum, int? maximum, string name = "")
        {
            _itemParser = itemParser;
            _separator = separator;
            _getContinuation = getContinuation;
            _minimum = minimum;
            _maximum = maximum;
            Name = name;
            _leftValue = new LeftValue(Name);
            _rightParser = getContinuation(_leftValue);
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _leftValue, _separator, _rightParser };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();
            _leftValue.Location = startCp.Location;

            // try to parse the continuation parser first (empty list)
            var result = _rightParser.Parse(state);
            if (result.Success)
            {
                // If there's no minimum, we can return success
                if (_minimum <= 1)
                    return result;

                // Rewind back to the beginning, the item and right parsers might share a prefix
                // and we might be able to keep going.
                startCp.Rewind();
            }

            int count = 0;
            int consumed = 0;
            while (true)
            {
                // 1. Parse an item. This must succeed or the list fails
                var nextItemResult = _itemParser.Parse(state);
                if (!nextItemResult.Success)
                {
                    startCp.Rewind();
                    return state.Fail(this, "Could not match");
                }

                _leftValue.Value.Add(nextItemResult.Value);
                consumed += nextItemResult.Consumed;

                var beforeContinuationCp = state.Input.Checkpoint();

                // 2. Try to parse the continuation. If this succeeds, we are done.
                var finalResult = _rightParser.Parse(state);
                if (finalResult.Success)
                {
                    if (count >= _minimum)
                        return state.Success(this, finalResult.Value, consumed + finalResult.Consumed, startCp.Location);

                    // Rewind to the space before the continuation was attempted, so we can continue
                    // parsing. The separator might have a shared prefix.
                    beforeContinuationCp.Rewind();
                }

                // If we are already at maximum, we cannot continue. Return failure.
                count++;
                if (_maximum.HasValue && count >= _maximum.Value)
                {
                    startCp.Rewind();
                    return state.Fail(this, "Matched too many items without a continuation");
                }

                // Try the separator and, if we have one, continue the loop.
                var separatorResult = _separator.Parse(state);
                if (!separatorResult.Success)
                {
                    // At this point the continuation and separator have both failed, so return
                    // failure
                    startCp.Rewind();
                    return state.Fail(this, "Could not match");
                }

                consumed += separatorResult.Consumed;
            }
        }

        public INamed SetName(string name) => new Parser(_itemParser, _separator, _getContinuation, _minimum, _maximum, name);

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
