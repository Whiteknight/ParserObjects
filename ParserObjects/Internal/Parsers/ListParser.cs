using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Executes an inner parser repeatedly, until it fails. All values are returned as a list.
/// Expects a number of matches between minimum and maximum values, inclusive.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class ListParser<TInput, TOutput> : IParser<TInput, IReadOnlyList<TOutput>>
{
    private readonly IParser<TInput, TOutput> _parser;
    private readonly IParser<TInput> _separator;

    public ListParser(IParser<TInput, TOutput> parser, IParser<TInput> separator, int minimum, int? maximum, string name = "")
    {
        Assert.ArgumentNotNull(parser, nameof(parser));

        Minimum = minimum < 0 ? 0 : minimum;
        Maximum = maximum;
        if (Maximum.HasValue && Maximum < Minimum)
            Maximum = Minimum;

        _parser = parser;
        _separator = separator;
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();
    public string Name { get; }
    public int Minimum { get; }
    public int? Maximum { get; }

    public IResult<IReadOnlyList<TOutput>> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var startCheckpoint = state.Input.Checkpoint();
        var items = new List<TOutput>();

        // We are parsing <List> := <Item> (<Separator> <Item>)*

        var initialItemResult = _parser.Parse(state);
        if (!initialItemResult.Success)
        {
            if (Minimum == 0)
                return state.Success(this, items, 0, startCheckpoint.Location);
            return state.Fail(this, $"Expected at least {Minimum} items but only found {items.Count}", startCheckpoint.Location);
        }

        items.Add(initialItemResult.Value);
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

            var result = _parser.Parse(state);
            if (!result.Success)
            {
                // If we fail the item here, we have to rewind to the position before the
                // separator.
                currentFailureCheckpoint.Rewind();
                break;
            }

            items.Add(result.Value);
            currentConsumed = result.Consumed;
            currentFailureCheckpoint = state.Input.Checkpoint();
        }

        // If we don't have at least Minimum items, we need to roll all the way back to the
        // beginning of the attempt
        if (Minimum > 0 && items.Count < Minimum)
        {
            startCheckpoint.Rewind();
            return state.Fail(this, $"Expected at least {Minimum} items but only found {items.Count}", startCheckpoint.Location);
        }

        var endConsumed = state.Input.Consumed;

        return state.Success(this, items, endConsumed - startCheckpoint.Consumed, startCheckpoint.Location);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

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

    public override string ToString() => DefaultStringifier.ToString("List", Name, Id);

    public INamed SetName(string name) => new ListParser<TInput, TOutput>(_parser, _separator, Minimum, Maximum, name);
}
