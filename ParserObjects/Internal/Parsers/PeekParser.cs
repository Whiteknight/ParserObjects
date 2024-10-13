using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Obtain the next item of input without advancing the input sequence.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class PeekParser<T> : IParser<T, T>
{
    public PeekParser(string name = "")
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

        var peek = state.Input.Peek();
        return state.Success(this, peek, 0);
    }

    Result<object> IParser<T>.Parse(IParseState<T> state) => Parse(state).AsObject();

    public bool Match(IParseState<T> state) => !state.Input.IsAtEnd;

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Peek", Name, Id);

    public INamed SetName(string name) => new PeekParser<T>(name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
