using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Obtain the next item of input without advancing the input sequence.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record PeekParser<T>(
    string Name = ""
) : IParser<T, T>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        if (state.Input.IsAtEnd)
            return state.Fail(this, "Expected any but found End");

        var peek = state.Input.Peek();
        return state.Success(this, peek, 0, state.Input.CurrentLocation);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public bool Match(IParseState<T> state) => !state.Input.IsAtEnd;

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Peek", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
