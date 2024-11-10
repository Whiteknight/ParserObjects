using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Executes an inner parser repeatedly with optional separators, until it fails. All values are
/// returned as a list. Expects a number of matches between minimum and maximum values, inclusive.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class Repetition<TInput>
{
    /* The InternalParser struct implements the core parsing logic. The various Parser classes
     * are just adaptors between InternalParser and IParser variants.
     */

    private readonly struct InternalParser<TParser, TResult, TItem>
        where TParser : IParser<TInput>
    {
        private readonly TParser _parser;
        private readonly IParser<TInput> _separator;
        private readonly Func<TParser, IParseState<TInput>, Result<TResult>> _getResult;
        private readonly Func<Result<TResult>, TItem> _getItem;

        public InternalParser(TParser parser, IParser<TInput> separator, Func<TParser, IParseState<TInput>, Result<TResult>> getResult, Func<Result<TResult>, TItem> getItem, int minimum, int? maximum)
        {
            _parser = parser;
            _separator = separator;
            _getResult = getResult;
            _getItem = getItem;
            Minimum = minimum < 0 ? 0 : minimum;
            Maximum = maximum;
            if (Maximum.HasValue && Maximum < Minimum)
                Maximum = Minimum;
        }

        public int Minimum { get; }

        public int? Maximum { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PartialResult<IReadOnlyList<TItem>> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            var startCheckpoint = state.Input.Checkpoint();
            var items = new List<TItem>();

            // We are parsing <List> := <Item> (<Separator> <Item>)*

            var initialItemResult = _getResult(_parser, state);
            if (!initialItemResult.Success)
            {
                if (Minimum == 0)
                    return new PartialResult<IReadOnlyList<TItem>>(items, 0);
                return new PartialResult<IReadOnlyList<TItem>>($"Expected at least {Minimum} items but only found {items.Count}");
            }

            var item = _getItem(initialItemResult);
            items.Add(item);
            int currentConsumed = initialItemResult.Consumed;
            var currentFailureCheckpoint = state.Input.Checkpoint();

            while (Maximum == null || items.Count < Maximum)
            {
                var separatorResult = _separator.Match(state);
                if (!separatorResult)
                    break;
                var separatorConsumed = state.Input.Consumed - currentFailureCheckpoint.Consumed;
                if (currentConsumed == 0 && separatorConsumed == 0 && items.Count >= Minimum)
                {
                    // An <Item> and <Separator> have consumed 0 inputs. We're going to break here
                    // to avoid getting into an infinite loop
                    // We don't need to rewind to before the separator, because it consumed nothing
                    break;
                }

                var result = _getResult(_parser, state);
                if (!result.Success)
                {
                    // If we fail the item here, we have to rewind to the position before the
                    // separator.
                    currentFailureCheckpoint.Rewind();
                    break;
                }

                item = _getItem(result);
                items.Add(item);
                currentConsumed = result.Consumed;
                currentFailureCheckpoint = state.Input.Checkpoint();
            }

            // If we don't have at least Minimum items, we need to roll all the way back to the
            // beginning of the attempt
            if (Minimum > 0 && items.Count < Minimum)
            {
                startCheckpoint.Rewind();
                return new PartialResult<IReadOnlyList<TItem>>($"Expected at least {Minimum} items but only found {items.Count}");
            }

            var endConsumed = state.Input.Consumed;

            return new PartialResult<IReadOnlyList<TItem>>(items, endConsumed - startCheckpoint.Consumed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            int count = 0;
            var startCheckpoint = state.Input.Checkpoint();

            // We are parsing <List> := <Item> (<Separator> <Item>)*

            var initialItemResult = _parser.Match(state);
            if (!initialItemResult)
                return Minimum == 0;

            count++;
            int currentConsumed = state.Input.Consumed - startCheckpoint.Consumed;
            var currentFailureCheckpoint = state.Input.Checkpoint();

            while (Maximum == null || count < Maximum)
            {
                var separatorResult = _separator.Match(state);
                if (!separatorResult)
                    break;
                var separatorConsumed = state.Input.Consumed - currentFailureCheckpoint.Consumed;
                if (currentConsumed == 0 && separatorConsumed == 0 && count >= Minimum)
                {
                    // An <Item> and <Separator> have consumed 0 inputs. We're going to break here
                    // to avoid getting into an infinite loop
                    // We don't need to rewind to before the separator, because it consumed nothing
                    break;
                }

                var beforeAttemptConsumed = state.Input.Consumed;

                var result = _parser.Match(state);
                if (!result)
                {
                    // If we fail the item here, we have to rewind to the position before the
                    // separator.
                    currentFailureCheckpoint.Rewind();
                    break;
                }

                count++;
                currentConsumed = state.Input.Consumed - beforeAttemptConsumed;
                currentFailureCheckpoint = state.Input.Checkpoint();
            }

            // If we don't have at least Minimum items, we need to roll all the way back to the
            // beginning of the attempt
            if (Minimum > 0 && count < Minimum)
            {
                startCheckpoint.Rewind();
                return false;
            }

            return true;
        }

        public IEnumerable<IParser> GetChildren() => new[] { _parser, _separator };
    }

    public sealed class Parser : IParser<TInput, IReadOnlyList<object>>
    {
        private readonly InternalParser<IParser<TInput>, object, object> _internal;

        public Parser(IParser<TInput> parser, IParser<TInput> separator, int minimum, int? maximum, string name = "")
        {
            Assert.ArgumentNotNull(parser);
            Assert.ArgumentNotNull(separator);

            _internal = new InternalParser<IParser<TInput>, object, object>(parser, separator, static (p, s) => p.Parse(s), static r => r.Value, minimum, maximum);

            Name = name;
        }

        private Parser(InternalParser<IParser<TInput>, object, object> internalData, string name)
        {
            _internal = internalData;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();
        public string Name { get; }
        public int Minimum => _internal.Minimum;
        public int? Maximum => _internal.Maximum;

        public Result<IReadOnlyList<object>> Parse(IParseState<TInput> state)
        {
            var partialResult = _internal.Parse(state);
            return Result.Create(this, partialResult);
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public bool Match(IParseState<TInput> state) => _internal.Match(state);

        public IEnumerable<IParser> GetChildren() => _internal.GetChildren();

        public override string ToString() => DefaultStringifier.ToString("List", Name, Id);

        public INamed SetName(string name) => new Parser(_internal, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IListPartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class Parser<TOutput> : IParser<TInput, IReadOnlyList<TOutput>>
    {
        private readonly InternalParser<IParser<TInput, TOutput>, TOutput, TOutput> _internal;

        public Parser(IParser<TInput, TOutput> parser, IParser<TInput> separator, int minimum, int? maximum, string name = "")
        {
            Assert.ArgumentNotNull(parser);
            Assert.ArgumentNotNull(separator);

            _internal = new InternalParser<IParser<TInput, TOutput>, TOutput, TOutput>(
                parser,
                separator,
                static (p, s) => p.Parse(s),
                static r => r.Value,
                minimum,
                maximum
            );

            Name = name;
        }

        private Parser(InternalParser<IParser<TInput, TOutput>, TOutput, TOutput> internalData, string name)
        {
            _internal = internalData;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();
        public string Name { get; }
        public int Minimum => _internal.Minimum;
        public int? Maximum => _internal.Maximum;

        public Result<IReadOnlyList<TOutput>> Parse(IParseState<TInput> state)
        {
            var partialResult = _internal.Parse(state);
            return Result.Create(this, partialResult);
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public bool Match(IParseState<TInput> state) => _internal.Match(state);

        public IEnumerable<IParser> GetChildren() => _internal.GetChildren();

        public override string ToString() => DefaultStringifier.ToString("List", Name, Id);

        public INamed SetName(string name) => new Parser<TOutput>(_internal, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<IListPartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
