using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Matches at the end of the input sequence. Fails if the input sequence is at any point
/// besides the end.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record SequenceFlagParser<TInput>(
    SequencePositionFlags Flags,
    string FailureMessage,
    string Name = ""
) : SimpleRecordParser<TInput, object>(Name), IParser<TInput, object>
{
    public override Result<object> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        return state.Input.Flags.Has(Flags)
            ? Result.Ok(this, Defaults.ObjectInstance, 0)
            : Result.Fail(this, FailureMessage + state.Input.Peek()!);
    }

    public override bool Match(IParseState<TInput> state)
        => state.Input.Flags.Has(Flags);

    public override string ToString() => DefaultStringifier.ToString("End", Name, Id);

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
