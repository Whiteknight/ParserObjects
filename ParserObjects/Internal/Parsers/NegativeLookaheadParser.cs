using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Negative lookahead parser. Tests the input to see if the inner parser matches. Return
/// success if the parser does not match, fail otherwise. Consumes no input.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record NegativeLookaheadParser<TInput>(
    IParser<TInput> Inner,
    string Name = ""
) : IParser<TInput, object>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public Result<object> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        var startCheckpoint = state.Input.Checkpoint();

        var result = Inner.Parse(state);
        if (!result.Success)
            return Result.Ok(this, Defaults.ObjectInstance, 0);

        startCheckpoint.Rewind();
        return Result.Fail(this, "Lookahead pattern existed but was not supposed to");
    }

    public bool Match(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        var startCheckpoint = state.Input.Checkpoint();

        var result = Inner.Match(state);
        if (!result)
            return true;

        startCheckpoint.Rewind();
        return false;
    }

    public IEnumerable<IParser> GetChildren() => new IParser[] { Inner };

    public override string ToString() => DefaultStringifier.ToString("NegativeLookahead", Name, Id);

    public INamed SetName(string name) => this with { Name = name };

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<ILookaheadPartialVisitor<TState>>()?.Accept(this, state);
    }
}
