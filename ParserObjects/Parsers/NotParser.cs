using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Invokes a parser and inverses the result success status. If the parser succeeds, return
/// Failure. Otherwise returns Success. Consumes no input in either case and returns no output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class NotParser<TInput> : IParser<TInput>
{
    private readonly IParser<TInput> _inner;

    public NotParser(IParser<TInput> inner, string name = "")
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
            return state.Success(this, Defaults.ObjectInstance, 0, result.Location);

        startCheckpoint.Rewind();
        return state.Fail(this, "Parser matched but was not supposed to");
    }

    public IEnumerable<IParser> GetChildren() => new[] { _inner };

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => new NotParser<TInput>(_inner, name);
}
