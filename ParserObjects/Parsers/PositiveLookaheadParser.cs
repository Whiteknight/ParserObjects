using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Does a lookahead to see if there is a match. Returns a success or failure result, but does
/// not consume any actual input.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record PositiveLookaheadParser<TInput>(
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
            return state.Fail(Inner, result.ErrorMessage, result.Location);

        startCheckpoint.Rewind();
        return state.Success(Inner, result.Value, 0, startCheckpoint.Location);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => new IParser[] { Inner };

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => new PositiveLookaheadParser<TInput>(Inner, name);
}
