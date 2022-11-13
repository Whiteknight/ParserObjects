using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Negative lookahead parser. Tests the input to see if the inner parser matches. Return
/// success if the parser does not match, fail otherwise. Consumes no input.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record NegativeLookaheadParser<TInput>(
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
            return state.Success(this, Defaults.ObjectInstance, 0);

        startCheckpoint.Rewind();
        return state.Fail(this, "Lookahead pattern existed but was not supposed to");
    }

    public IEnumerable<IParser> GetChildren() => new IParser[] { Inner };

    public override string ToString() => DefaultStringifier.ToString("NegativeLookahead", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
