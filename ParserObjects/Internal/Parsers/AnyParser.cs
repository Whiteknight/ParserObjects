using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Matches any input item that isn't the end of input. Consumes exactly one input item and
/// returns it.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record AnyParser<T>(
    string Name = ""
) : SimpleRecordParser<T, T>(Name), IParser<T, T>
{
    public override Result<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state);
        if (state.Input.IsAtEnd)
            return Result<T>.Fail(this, "Expected any but found End.");

        var next = state.Input.GetNext();
        return state.Success(this, next, 1);
    }

    public override bool Match(IParseState<T> state)
    {
        if (state.Input.IsAtEnd)
            return false;
        state.Input.GetNext();
        return true;
    }

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
