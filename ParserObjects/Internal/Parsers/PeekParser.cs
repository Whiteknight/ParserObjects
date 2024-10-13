using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Obtain the next item of input without advancing the input sequence.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record PeekParser<T> : SimpleRecordParser<T, T>, IParser<T, T>
{
    public PeekParser(string name = "")
        : base(name)
    {
    }

    public override Result<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state);
        if (state.Input.IsAtEnd)
            return Result<T>.Fail(this, "Expected any but found End.");

        var peek = state.Input.Peek();
        return state.Success(this, peek, 0);
    }

    public override bool Match(IParseState<T> state) => !state.Input.IsAtEnd;

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
