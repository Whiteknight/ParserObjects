using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// A constant left value for places where a left value is used to start a parse.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public class LeftValue<TInput, TOutput> : IParser<TInput, TOutput>
{
    public LeftValue(string name)
    {
        Name = name;
    }

    public TOutput? Value { get; set; }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public IResult<TOutput> Parse(IParseState<TInput> state) => state.Success(this, Value!, 0);

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => state.Success(this, Value!, 0);

    public bool Match(IParseState<TInput> state) => true;

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename inner value parser");

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
    }
}
