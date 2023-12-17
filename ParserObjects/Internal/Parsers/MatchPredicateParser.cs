using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Returns the next input item if it satisfies a predicate, failure otherwise. Notice that
/// the end sentinel will be made available to the predicate and may match.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TData"></typeparam>
public sealed class MatchPredicateParser<T, TData> : IParser<T, T>
{
    /* The ReadAtEnd flag determines if we should attempt to read and match an end sentinel when
     * the sequence is at end-of-input. This is useful in some cases where we want to have custom
     * end parsing/handling, but many cases don't need to read the end sentinel and can bail out
     * and end.
     */

    private readonly TData _data;
    private readonly Func<T, TData, bool> _predicate;
    private readonly bool _readAtEnd;
    private readonly IResult<T> _failure;

    public MatchPredicateParser(TData data, Func<T, TData, bool> predicate, string name = "", bool readAtEnd = true)
    {
        _data = data;
        _predicate = predicate;
        Name = name;
        _readAtEnd = readAtEnd;
        _failure = new FailureResult<T>(this, "Next item does not match the predicate");
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public IResult<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state);

        var startConsumed = state.Input.Consumed;

        if (!_readAtEnd && state.Input.IsAtEnd)
            return _failure;

        var next = state.Input.Peek();
        if (next == null || !_predicate(next, _data))
            return _failure;

        state.Input.GetNext();
        return state.Success(this, next, state.Input.Consumed - startConsumed);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public bool Match(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state);

        if (!_readAtEnd && state.Input.IsAtEnd)
            return false;

        var next = state.Input.Peek();
        if (next == null || !_predicate(next, _data))
            return false;

        state.Input.GetNext();
        return true;
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("MatchPredicate", Name, Id);

    public INamed SetName(string name) => new MatchPredicateParser<T, TData>(_data, _predicate, name, _readAtEnd);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
