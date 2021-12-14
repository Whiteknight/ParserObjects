using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Negative lookahead parser. Tests the input to see if the inner parser matches. Return
/// success if the parser does not match, fail otherwise. Consumes no input.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class NegativeLookaheadParser<TInput> : IParser<TInput>
{
    private readonly IParser<TInput> _inner;

    public NegativeLookaheadParser(IParser<TInput> inner, string name = "")
    {
        Assert.ArgumentNotNull(inner, nameof(inner));
        _inner = inner;
        Name = name;
    }

    public string Name { get; }

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();

        var result = _inner.Parse(state);
        if (!result.Success)
            return state.Success(this, Defaults.ObjectInstance, 0);

        startCheckpoint.Rewind();
        return state.Fail(this, "Lookahead pattern existed but was not supposed to");
    }

    public IEnumerable<IParser> GetChildren() => new IParser[] { _inner };

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => new NegativeLookaheadParser<TInput>(_inner, name);
}
