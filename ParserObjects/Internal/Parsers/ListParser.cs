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
        var currentFailureCheckpoint = startCheckpoint;
        var items = new List<TOutput>();

        int consumed = 0;
        int separatorConsumed = 0;
        while (Maximum == null || items.Count < Maximum)
        {
            var result = _parser.Parse(state);
            if (!result.Success)
            {
                currentFailureCheckpoint.Rewind();
                break;
            }

            consumed += result.Consumed + separatorConsumed;
            items.Add(result.Value);
            if (items.Count >= Minimum && result.Consumed == 0)
                break;

            // If we Succeed the separator but fail the subsequent item, we need to roll back to
            // the point directly before the separator. Notice that this probably doesn't imply
            // the failure of the entire List parse
            currentFailureCheckpoint = state.Input.Checkpoint();
            var separatorResult = _separator.Parse(state);
            if (!separatorResult.Success)
                break;

            separatorConsumed = separatorResult.Consumed;
        }

        // If we don't have at least Minimum items, we need to roll all the way back to the
        // beginning of the attempt
        if (Minimum > 0 && items.Count < Minimum)
        {
            startCheckpoint.Rewind();
            return state.Fail(this, $"Expected at least {Minimum} items but only found {items.Count}", startCheckpoint.Location);
        }

        return state.Success(this, items, consumed, startCheckpoint.Location);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    // TODO: Need to update BNF output to account for separator
    public IEnumerable<IParser> GetChildren() => new[] { _parser, _separator };

    public override string ToString() => DefaultStringifier.ToString("LimitedList", Name, Id);

    public INamed SetName(string name) => new ListParser<TInput, TOutput>(_parser, _separator, Minimum, Maximum, name);
}
