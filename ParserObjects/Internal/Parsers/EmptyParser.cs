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
    public EmptyParser(string name = "")
    {
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public Result<object> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        return Result<object>.Ok(this, Defaults.ObjectInstance, 0);
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
