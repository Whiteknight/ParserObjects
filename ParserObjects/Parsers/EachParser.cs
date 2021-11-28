using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

public sealed class EachParser<TInput, TOutput> : IMultiParser<TInput, TOutput>
{
    private readonly IReadOnlyList<IParser<TInput, TOutput>> _parsers;

    public EachParser(IEnumerable<IParser<TInput, TOutput>> parsers)
    {
        _parsers = parsers.OrEmptyIfNull().ToList();
        Name = string.Empty;
    }

    public string Name { get; set; }

    public IEnumerable<IParser> GetChildren() => _parsers;

    public IMultiResult<TOutput> Parse(IParseState<TInput> state)
    {
        var startCheckpoint = state.Input.Checkpoint();
        var results = new List<IResultAlternative<TOutput>>();

        foreach (var parser in _parsers)
        {
            var result = ParseOne(parser, state, startCheckpoint);
            results.Add(result);
            startCheckpoint.Rewind();
            continue;
        }

        return new MultiResult<TOutput>(this, startCheckpoint.Location, startCheckpoint, results);
    }

    private static IResultAlternative<TOutput> ParseOne(IParser<TInput, TOutput> parser, IParseState<TInput> state, ISequenceCheckpoint startCheckpoint)
    {
        var result = parser.Parse(state);
        if (result.Success)
        {
            var endCheckpoint = state.Input.Checkpoint();
            return new SuccessResultAlternative<TOutput>(result.Value, result.Consumed, endCheckpoint);
        }

        return new FailureResultAlternative<TOutput>(result.ErrorMessage, startCheckpoint);
    }

    IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public override string ToString() => DefaultStringifier.ToString(this);
}
