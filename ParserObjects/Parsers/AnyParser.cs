using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Matches any input item that isn't the end of input. Consumes exactly one input item and
/// returns it.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class AnyParser<T> : IParser<T, T>
{
    public AnyParser()
    {
        Name = string.Empty;
    }

    public string Name { get; set; }

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

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);
}
