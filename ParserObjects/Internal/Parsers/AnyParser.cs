using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Matches any input item that isn't the end of input. Consumes exactly one input item and
/// returns it.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record AnyParser<T>(
    bool Peek,
    string Name = ""
) : SimpleRecordParser<T, T>(Name), IParser<T, T>
{
    public override Result<T> Parse(IParseState<T> state)
    {
        Assert.NotNull(state);
        if (state.Input.IsAtEnd)
            return Result.Fail(this, "Expected any but found End.");

        var next = GetNext(state);
        return Result.Ok(this, next, 1);
    }

    public override bool Match(IParseState<T> state)
    {
        if (state.Input.IsAtEnd)
            return false;

        GetNext(state);
        return true;
    }

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }

    private T GetNext(IParseState<T> state)
        => Peek ? state.Input.Peek() : state.Input.GetNext();
}
