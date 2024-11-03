using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parser to convert an MultiResult into an Result by selecting the best result alternative
/// using user-supplied criteria.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed record SelectParser<TInput, TOutput>(
    IMultiParser<TInput, TOutput> Initial,
    SelectResultFromMultiResult<TOutput> Selector,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public Result<TOutput> Parse(IParseState<TInput> state)
    {
        var multi = Initial.Parse(state);
        if (!multi.Success)
            return state.Fail(this, "Parser returned no valid results");

        var selected = Selector(multi);
        if (!selected.Success)
            return state.Fail(this, "No alternative selected, or no matching successful value could be found");

        selected.Continuation.Rewind();
        return multi.ToResult(selected);
    }

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public override string ToString() => DefaultStringifier.ToString("Select", Name, Id);

    public IEnumerable<IParser> GetChildren() => new[] { Initial };

    public INamed SetName(string name) => this with { Name = name };

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMultiPartialVisitor<TState>>()?.Accept(this, state);
    }
}
