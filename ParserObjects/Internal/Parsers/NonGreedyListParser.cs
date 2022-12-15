using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public static class NonGreedyList<TInput, TItem, TOutput>
{
    public class Parser : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TItem> _itemParser;
        private readonly IParser<TInput> _separator;
        private readonly Func<IParser<TInput, IReadOnlyList<TItem>>, IParser<TInput, TOutput>> _getContinuation;
        private readonly LeftValue _leftValue;
        private readonly IParser<TInput, TOutput> _rightParser;

        public Parser(IParser<TInput, TItem> itemParser, IParser<TInput> separator, Func<IParser<TInput, IReadOnlyList<TItem>>, IParser<TInput, TOutput>> getContinuation, int minimum, int? maximum, string name = "")
        {
            _itemParser = itemParser;
            _separator = separator;
            _getContinuation = getContinuation;
            Minimum = minimum;
            Maximum = maximum;
            Name = name;
            _leftValue = new LeftValue(Name);
            _rightParser = getContinuation(_leftValue);
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public int Minimum { get; }
        public int? Maximum { get; }

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
                if (Minimum <= 1)
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
                    if (count >= Minimum)
                        return state.Success(this, finalResult.Value, consumed + finalResult.Consumed, startCp.Location);

                    // Rewind to the space before the continuation was attempted, so we can continue
                    // parsing. The separator might have a shared prefix.
                    beforeContinuationCp.Rewind();
                }

                // If we are already at maximum, we cannot continue. Return failure.
                count++;
                if (Maximum.HasValue && count >= Maximum.Value)
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

        public INamed SetName(string name) => new Parser(_itemParser, _separator, _getContinuation, Minimum, Maximum, name);

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString("NonGreedyList", Name, Id);

        private class LeftValue : IParser<TInput, IReadOnlyList<TItem>>, IHiddenInternalParser
        {
            public LeftValue(string name)
            {
                Name = name;
                Value = new List<TItem>();
            }

            public List<TItem> Value { get; }

            public Location Location { get; set; }

            public int Id { get; } = UniqueIntegerGenerator.GetNext();

            public string Name { get; }

            public IResult<IReadOnlyList<TItem>> Parse(IParseState<TInput> state) => state.Success(this, Value, 0, Location);

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => state.Success(this, Value!, 0, Location);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);

            public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename the internal value parser");
        }
    }
}
