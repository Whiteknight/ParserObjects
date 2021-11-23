using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Does a lookahead to see if there is a match. Returns a success or failure result, but does
/// not consume any actual input.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public class PositiveLookaheadParser<TInput> : IParser<TInput>
{
    private readonly IParser<TInput> _inner;

    public PositiveLookaheadParser(IParser<TInput> inner)
    {
        Assert.ArgumentNotNull(inner, nameof(inner));
        _inner = inner;
        Name = string.Empty;
    }

    public string Name { get; set; }

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        var result = _inner.Parse(state);
        if (!result.Success)
            return state.Fail(_inner, result.ErrorMessage, result.Location);

        startCheckpoint.Rewind();
        return state.Success(_inner, result.Value, 0, startCheckpoint.Location);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

    public override string ToString() => DefaultStringifier.ToString(this);
}
