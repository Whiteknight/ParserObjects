using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Invokes a parser and inverses the result success status. If the parser succeeds, return
/// Failure. Otherwise returns Success. Consumes no input in either case and returns no output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record NotParser<TInput>(
    IParser<TInput> Inner,
    string Name = ""
) : IParser<TInput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        var result = Inner.Parse(state);
        if (!result.Success)
            return state.Success(this, Defaults.ObjectInstance, 0, result.Location);

        startCheckpoint.Rewind();
        return state.Fail(this, "Parser matched but was not supposed to");
    }

    public bool Match(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        var result = Inner.Match(state);
        if (!result)
            return true;

        startCheckpoint.Rewind();
        return false;
    }

    public IEnumerable<IParser> GetChildren() => new[] { Inner };

    public override string ToString() => DefaultStringifier.ToString("Not", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
