using System.Collections.Generic;
using ParserObjects.Internal.Visitors;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Does a lookahead to see if there is a match. Returns a success or failure result, but does
/// not consume any actual input.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record PositiveLookaheadParser<TInput>(
    IParser<TInput> Inner,
    string Name = ""
) : IParser<TInput, object>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public Result<object> Parse(IParseState<TInput> state)
    {
        NotNull(state);
        var startCheckpoint = state.Input.Checkpoint();
        var result = Inner.Parse(state);
        if (!result.Success)
            return result;

        startCheckpoint.Rewind();
        return result with { Consumed = 0 };
    }

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    public bool Match(IParseState<TInput> state)
    {
        NotNull(state);
        var startCheckpoint = state.Input.Checkpoint();
        var result = Inner.Match(state);
        if (!result)
            return false;

        startCheckpoint.Rewind();
        return true;
    }

    public IEnumerable<IParser> GetChildren() => [Inner];

    public override string ToString() => DefaultStringifier.ToString("PositiveLookahead", Name, Id);

    public INamed SetName(string name) => new PositiveLookaheadParser<TInput>(Inner, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<ILookaheadPartialVisitor<TState>>()?.Accept(this, state);
    }
}
