using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Matches any input item that isn't the end of input. Consumes exactly one input item and
/// returns it.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class AnyParser<T> : IParser<T, T>
{
    public AnyParser(string name = "")
    {
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public Result<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state);
        if (state.Input.IsAtEnd)
            return Result<T>.Fail(this, "Expected any but found End.");

        var next = state.Input.GetNext();
        return state.Success(this, next, 1);
    }

    Result<object> IParser<T>.Parse(IParseState<T> state) => Parse(state).AsObject();

    public bool Match(IParseState<T> state)
    {
        if (state.Input.IsAtEnd)
            return false;
        state.Input.GetNext();
        return true;
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Any", Name, Id);

    public INamed SetName(string name) => new AnyParser<T>(name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
