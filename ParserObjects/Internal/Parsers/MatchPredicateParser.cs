using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Returns the next input item if it satisfies a predicate, failure otherwise. Notice that
/// the end sentinel will be made available to the predicate and may match.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class MatchPredicateParser<T> : IParser<T, T>
{
    private readonly Func<T, bool> _predicate;
    private readonly IResult<T> _failure;

    public MatchPredicateParser(Func<T, bool> predicate, string name = "")
    {
        _predicate = predicate;
        Name = name;
        _failure = new FailureResult<T>(this, "Next item does not match the predicate");
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public IResult<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var startConsumed = state.Input.Consumed;

        var next = state.Input.Peek();
        if (next == null || !_predicate(next))
            return _failure;

        state.Input.GetNext();
        return state.Success(this, next, state.Input.Consumed - startConsumed);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public bool Match(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var next = state.Input.Peek();
        if (next == null || !_predicate(next))
            return false;

        state.Input.GetNext();
        return true;
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("MatchPredicate", Name, Id);

    public INamed SetName(string name) => new MatchPredicateParser<T>(_predicate, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
