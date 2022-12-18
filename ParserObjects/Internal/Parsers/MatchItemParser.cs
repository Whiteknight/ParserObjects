using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public record MatchItemParser<T>(
    T? Item,
    string Name = ""
) : IParser<T, T>
{
    public int Id => UniqueIntegerGenerator.GetNext();

    public bool Match(IParseState<T> state)
    {
        if (state.Input.IsAtEnd)
            return false;
        if (!Equals(Item, state.Input.Peek()))
            return false;
        state.Input.GetNext();
        return true;
    }

    public IResult<T> Parse(IParseState<T> state)
    {
        if (state.Input.IsAtEnd)
            return state.Fail(this, "Input sequence is at end");
        if (!Equals(Item, state.Input.Peek()))
            return state.Fail(this, "Items do not match");
        var location = state.Input.CurrentLocation;
        var value = state.Input.GetNext();
        return state.Success(this, value, 1, location);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public INamed SetName(string name) => this with { Name = name };

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();
}
