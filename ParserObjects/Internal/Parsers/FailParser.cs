using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Returns unconditional failure, optionally with a helpful error message.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed record FailParser<TInput, TOutput>(
    string ErrorMessage = "Fail",
    string Name = ""
) : SimpleRecordParser<TInput, TOutput>(Name), IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
{
    public override Result<TOutput> Parse(IParseState<TInput> state)
        => Result.Fail(this, ErrorMessage);

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state)
        => Result.Fail<object>(this, ErrorMessage);

    MultiResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        Assert.NotNull(state);
        return MultiResult<TOutput>.FromSingleFailure(this, state.Input.Checkpoint(), ErrorMessage);
    }

    MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state)
    {
        Assert.NotNull(state);
        return MultiResult<object>.FromSingleFailure(this, state.Input.Checkpoint(), ErrorMessage);
    }

    public override bool Match(IParseState<TInput> state) => false;

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
    }
}
