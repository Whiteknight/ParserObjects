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

        public IEnumerable<IParser> GetChildren() => new IParser[] { _itemParser, _separator, _rightParser };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();
            _leftValue.Location = startCp.Location;

            // We are parsing <List> := <Right> | <Item> (<Separator> <Item>)*? <Right>

            // try to parse the continuation parser first (empty list)
            if (Minimum == 0)
            {
                var result = _rightParser.Parse(state);
                if (result.Success)
                    return result;
            }

            // Try to match the first item
            var nextItemResult = _itemParser.Parse(state);
            if (!nextItemResult.Success)
                return state.Fail(this, $"Expected at least {Minimum} items in the list but found 0.");

            _leftValue.Value.Add(nextItemResult.Value);
            int count = 1;

            // First make sure we account for Minimum items. We don't even need to attempt the
            // Right parser at this time.
            while (count < Minimum)
            {
                // Try the separator and, if we have one, continue the loop.
                var separatorResult = _separator.Match(state);
                if (!separatorResult)
                {
                    // If the separator fails, and we're still below the minimum, it's a failure
                    startCp.Rewind();
                    return state.Fail(this, $"Expected at least {Minimum} items in the list but found {count}.");
                }

                // Parse an item. This must succeed or the list fails
                nextItemResult = _itemParser.Parse(state);
                if (!nextItemResult.Success)
                {
                    startCp.Rewind();
                    return state.Fail(this, $"Expected at least {Minimum} items in the list but found {count}.");
                }

                _leftValue.Value.Add(nextItemResult.Value);
                count++;
            }

            while (true)
            {
                // First, see if we can match the Right parser and bail out
                var result = _rightParser.Parse(state);
                if (result.Success)
                    return state.Success(this, result.Value, state.Input.Consumed - startCp.Consumed, startCp.Location);

                if (Maximum.HasValue && count >= Maximum)
                    return state.Fail(this, $"Found maximum {Maximum} items but could not complete list.");

                int cycleStartConsumed = state.Input.Consumed;

                // Search for a separator to start the next iteration
                var separatorResult = _separator.Match(state);
                if (!separatorResult)
                {
                    // Right failed and Separator failed, so there's nowhere left to go. Fail.
                    startCp.Rewind();
                    return state.Fail(this, "Could not continue or complete the list.");
                }

                // Parse an item. This must succeed or the list fails
                nextItemResult = _itemParser.Parse(state);
                if (!nextItemResult.Success)
                {
                    startCp.Rewind();
                    return state.Fail(this, "Found a separator but could not find the next item.");
                }

                int cycleEndConsumed = state.Input.Consumed;
                int consumedThisCycle = cycleEndConsumed - cycleStartConsumed;
                if (consumedThisCycle == 0)
                {
                    // Separator and Item have consumed no inputs. Since we're already above
                    // Minimum items, we won't keep looping. We also know that Right doesn't match
                    // at this position, so it's a failure
                    return state.Fail(this, "Consumed zero inputs but could not complete list. Failing to avoid an infinite loop.");
                }

                _leftValue.Value.Add(nextItemResult.Value);
                count++;
            }
        }

        public INamed SetName(string name) => new Parser(_itemParser, _separator, _getContinuation, Minimum, Maximum, name);

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state)
        {
            var startCp = state.Input.Checkpoint();

            // We are matching <List> := <Right> | <Item> (<Separator> <Item>)*? <Right>

            // try to match the continuation parser first (empty list)
            if (Minimum == 0)
            {
                var result = _rightParser.Match(state);
                if (result)
                    return true;
            }

            // Try to match the first item
            var nextItemResult = _itemParser.Match(state);
            if (!nextItemResult)
                return false;

            int count = 1;

            // First make sure we account for Minimum items. We don't even need to attempt the
            // Right parser at this time.
            while (count < Minimum)
            {
                // Try the separator and, if we have one, continue the loop.
                var separatorResult = _separator.Match(state);
                if (!separatorResult)
                {
                    // If the separator fails, and we're still below the minimum, it's a failure
                    startCp.Rewind();
                    return false;
                }

                // Parse an item. This must succeed or the list fails
                nextItemResult = _itemParser.Match(state);
                if (!nextItemResult)
                {
                    startCp.Rewind();
                    return false;
                }

                count++;
            }

            while (true)
            {
                // First, see if we can match the Right parser and bail out
                var result = _rightParser.Match(state);
                if (result)
                    return true;

                if (Maximum.HasValue && count >= Maximum)
                    return false;

                int cycleStartConsumed = state.Input.Consumed;

                // Search for a separator to start the next iteration
                var separatorResult = _separator.Match(state);
                if (!separatorResult)
                {
                    // Right failed and Separator failed, so there's nowhere left to go. Fail.
                    startCp.Rewind();
                    return false;
                }

                // Parse an item. This must succeed or the list fails
                nextItemResult = _itemParser.Match(state);
                if (!nextItemResult)
                {
                    startCp.Rewind();
                    return false;
                }

                int cycleEndConsumed = state.Input.Consumed;
                int consumedThisCycle = cycleEndConsumed - cycleStartConsumed;
                if (consumedThisCycle == 0)
                {
                    // Separator and Item have consumed no inputs. Since we're already above
                    // Minimum items, we won't keep looping. We also know that Right doesn't match
                    // at this position, so it's a failure
                    return false;
                }

                count++;
            }
        }

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

            public bool Match(IParseState<TInput> state) => true;

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);

            public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename the internal value parser");
        }
    }
}
