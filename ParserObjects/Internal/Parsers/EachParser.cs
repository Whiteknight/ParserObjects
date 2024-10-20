using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Executes a number of IParser instances from the current location and returns all of their
/// results as an MultiResult.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
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

    public MultiResult<TOutput> Parse(IParseState<TInput> state)
    {
        var startCheckpoint = state.Input.Checkpoint();
        var results = new List<ResultAlternative<TOutput>>();

        foreach (var parser in _parsers)
        {
            var result = ParseOne(parser, state, startCheckpoint);
            results.Add(result);
            startCheckpoint.Rewind();
        }

        return new MultiResult<TOutput>(this, startCheckpoint, results);
    }

    private static ResultAlternative<TOutput> ParseOne(IParser<TInput, TOutput> parser, IParseState<TInput> state, SequenceCheckpoint startCheckpoint)
    {
        var result = parser.Parse(state);
        if (result.Success)
        {
            var endCheckpoint = state.Input.Checkpoint();
            return ResultAlternative<TOutput>.Ok(result.Value, result.Consumed, endCheckpoint);
        }

        return ResultAlternative<TOutput>.Failure(result.ErrorMessage, startCheckpoint);
    }

    MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    public IEnumerable<IParser> GetChildren() => _parsers;

    public override string ToString() => DefaultStringifier.ToString("Each", Name, Id);

    public INamed SetName(string name) => new EachParser<TInput, TOutput>(_parsers, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMultiPartialVisitor<TState>>()?.Accept(this, state);
    }
}
