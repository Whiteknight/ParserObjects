using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

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
            return state.Fail(Inner, result.ErrorMessage);

        startCheckpoint.Rewind();
        return state.Success(Inner, result.Value, 0);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        var result = Inner.Match(state);
        if (!result)
            return false;

        startCheckpoint.Rewind();
        return true;
    }

    public IEnumerable<IParser> GetChildren() => new IParser[] { Inner };

    public override string ToString() => DefaultStringifier.ToString("PositiveLookahead", Name, Id);

    public INamed SetName(string name) => new PositiveLookaheadParser<TInput>(Inner, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<ILookaheadPartialVisitor<TState>>()?.Accept(this, state);
    }
}
