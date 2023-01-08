using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public sealed class MatchItemParser<T> : IParser<T, T>
{
    private readonly IResult<T> _eofResult;
    private readonly IResult<T> _noMatchResult;

    public MatchItemParser(T? item, string name = "")
    {
        Item = item;
        Name = name;
        _eofResult = new FailureResult<T>(this, "Input sequence is at end.", default);
        _noMatchResult = new FailureResult<T>(this, "Items do not match", default);
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }
    public T? Item { get; }

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
            return _eofResult;
        if (!Equals(Item, state.Input.Peek()))
            return _noMatchResult;
        var value = state.Input.GetNext();
        return state.Success(this, value, 1);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public INamed SetName(string name) => new MatchItemParser<T>(Item, name);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);
}
