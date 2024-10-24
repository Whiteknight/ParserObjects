using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// The empty parser, consumes no input and always returns success.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record EmptyParser<TInput>(
    string Name = ""
) : SimpleRecordParser<TInput, object>(Name), IParser<TInput, object>
{
    public override Result<object> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        return Result<object>.Ok(this, Defaults.ObjectInstance, 0);
    }

    public override bool Match(IParseState<TInput> state) => true;

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
