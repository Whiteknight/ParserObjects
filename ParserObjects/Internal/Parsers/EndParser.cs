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
    /* We do not cache the error result because the error message (currently) contains the item
     * which was found in the input sequence. End-checking happens relatively rarely in most
     * parsers so we (probably) don't need to optimize that case any further.
     */

    public override Result<object> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        return state.Input.Flags.Has(Flags)
            ? Result<object>.Ok(this, Defaults.ObjectInstance, 0)
            : state.Fail(this, FailureMessage + state.Input.Peek()!);
    }

    public override bool Match(IParseState<TInput> state)
        => state.Input.Flags.Has(Flags);

    public override string ToString() => DefaultStringifier.ToString("End", Name, Id);

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
