using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Matches any input item that isn't the end of input. Consumes exactly one input item and
/// returns it.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record AnyParser<T>(
    string Name = ""
) : IParser<T, T>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        if (state.Input.IsAtEnd)
            return state.Fail(this, "Expected any but found End");

        var location = state.Input.CurrentLocation;
        var next = state.Input.GetNext();
        return state.Success(this, next, 1, location);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public bool Match(IParseState<T> state)
    {
        if (state.Input.IsAtEnd)
            return false;
        state.Input.GetNext();
        return true;
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Any", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
