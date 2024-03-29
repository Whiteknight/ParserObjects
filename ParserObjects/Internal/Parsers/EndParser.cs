﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Matches at the end of the input sequence. Fails if the input sequence is at any point
/// besides the end.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class EndParser<TInput> : IParser<TInput>
{
    /* We do not cache the error result because the error message (currently) contains the item
     * which was found in the input sequence. End-checking happens relatively rarely in most
     * parsers so we (probably) don't need to optimize that case any further.
     */

    private readonly IResult _success;

    public EndParser(string name = "")
    {
        Name = name;
        _success = new SuccessResult<object>(this, Defaults.ObjectInstance, 0);
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        return state.Input.IsAtEnd
            ? _success
            : state.Fail(this, "Expected end of Input but found " + state.Input.Peek()!);
    }

    public bool Match(IParseState<TInput> state) => state.Input.IsAtEnd;

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("End", Name, Id);

    public INamed SetName(string name) => new EndParser<TInput>(name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
