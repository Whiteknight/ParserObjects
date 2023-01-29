using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// The empty parser, consumes no input and always returns success.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class EmptyParser<TInput> : IParser<TInput>
{
    private readonly IResult _result;

    public EmptyParser(string name = "")
    {
        _result = new SuccessResult<object>(this, Defaults.ObjectInstance, 0);
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        return _result;
    }

    public bool Match(IParseState<TInput> state) => true;

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Empty", Name, Id);

    public INamed SetName(string name) => new EmptyParser<TInput>(name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
