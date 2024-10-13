using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Attempts to match a predicate condition and, invokes a specified parser on success or
/// failure.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed record IfParser<TInput, TOutput>(
    IParser<TInput> Predicate,
    IParser<TInput, TOutput> OnSuccess,
    IParser<TInput, TOutput> OnFailure,
    string Name = ""
) : SimpleRecordParser<TInput, TOutput>(Name), IParser<TInput, TOutput>
{
    public override Result<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        var cp = state.Input.Checkpoint();
        var result = Predicate.Parse(state);
        return Parse(state, result.Success ? OnSuccess : OnFailure, cp, result.Consumed);
    }

    private static Result<TOutput> Parse(IParseState<TInput> state, IParser<TInput, TOutput> parser, SequenceCheckpoint cp, int predicateConsumed)
    {
        var thenResult = parser.Parse(state);
        if (thenResult.Success)
            return state.Success(parser, thenResult.Value, predicateConsumed + thenResult.Consumed);
        cp.Rewind();
        return state.Fail(parser, thenResult.ErrorMessage);
    }

    public override IEnumerable<IParser> GetChildren() => new IParser[] { Predicate, OnSuccess, OnFailure };

    public override string ToString() => DefaultStringifier.ToString("If", Name, Id);

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<ILogicalPartialVisitor<TState>>()?.Accept(this, state);
    }
}
