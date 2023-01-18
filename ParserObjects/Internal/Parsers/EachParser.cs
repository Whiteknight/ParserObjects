using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public sealed class EachParser<TInput, TOutput> : IMultiParser<TInput, TOutput>
{
    private readonly IReadOnlyList<IParser<TInput, TOutput>> _parsers;

    public EachParser(IReadOnlyList<IParser<TInput, TOutput>> parsers, string name)
    {
        _parsers = parsers;
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; set; }

    public IMultiResult<TOutput> Parse(IParseState<TInput> state)
    {
        var startCheckpoint = state.Input.Checkpoint();
        var results = new List<IResultAlternative<TOutput>>();

        foreach (var parser in _parsers)
        {
            var result = ParseOne(parser, state, startCheckpoint);
            results.Add(result);
            startCheckpoint.Rewind();
        }

        return new MultiResult<TOutput>(this, startCheckpoint, results);
    }

    private static IResultAlternative<TOutput> ParseOne(IParser<TInput, TOutput> parser, IParseState<TInput> state, SequenceCheckpoint startCheckpoint)
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

    public IEnumerable<IParser> GetChildren() => _parsers;

    public override string ToString() => DefaultStringifier.ToString("Each", Name, Id);

    public INamed SetName(string name) => new EachParser<TInput, TOutput>(_parsers, name);
}
