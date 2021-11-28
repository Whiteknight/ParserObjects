using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Tests several parsers sequentially. If all of them succeed return Success. If any Fail,
/// return Failure. Consumes input but returns no explicit output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class AndParser<TInput> : IParser<TInput>
{
    private readonly IReadOnlyList<IParser<TInput>> _parsers;

    public AndParser(params IParser<TInput>[] parsers)
    {
        Assert.ArrayNotNullAndContainsNoNulls(parsers, nameof(parsers));
        _parsers = parsers;
        Name = string.Empty;
    }

    public string Name { get; set; }

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        foreach (var parser in _parsers)
        {
            var result = parser.Parse(state);
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }
        }

        var consumed = state.Input.Consumed - startCheckpoint.Consumed;
        return state.Success(this, Defaults.ObjectInstance, consumed, startCheckpoint.Location);
    }

    public IEnumerable<IParser> GetChildren() => _parsers;

    public override string ToString() => DefaultStringifier.ToString(this);
}
