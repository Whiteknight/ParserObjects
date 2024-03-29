﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Match a single input against a given item using the Equals() method for default equality
/// semantics. Returns success if the items match, failure otherwise.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class MatchItemParser<T> : IParser<T, T>
{
    /* Since failure results are always identical, we can cache instances of them and return them
     * instead of allocating a new one on every potential runthrough.
     */

    private readonly IResult<T> _eofResult;
    private readonly IResult<T> _noMatchResult;
    private readonly bool _readAtEnd;

    public MatchItemParser(T? item, string name = "", bool readAtEnd = true)
    {
        Item = item;
        Name = name;
        _readAtEnd = readAtEnd;
        _eofResult = new FailureResult<T>(this, "Input sequence is at end.", default);
        _noMatchResult = new FailureResult<T>(this, "Items do not match", default);
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }
    public T? Item { get; }

    public bool Match(IParseState<T> state)
    {
        if (!_readAtEnd && state.Input.IsAtEnd)
            return false;
        if (!Equals(Item, state.Input.Peek()))
            return false;
        state.Input.GetNext();
        return true;
    }

    public IResult<T> Parse(IParseState<T> state)
    {
        if (!_readAtEnd && state.Input.IsAtEnd)
            return _eofResult;
        if (!Equals(Item, state.Input.Peek()))
            return _noMatchResult;
        int startConsumed = state.Input.Consumed;
        var value = state.Input.GetNext();
        return state.Success(this, value, state.Input.Consumed - startConsumed);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public INamed SetName(string name) => new MatchItemParser<T>(Item, name);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
