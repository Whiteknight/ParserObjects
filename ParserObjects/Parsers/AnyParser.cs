using System.Collections.Generic;
using System.Linq;
using ParserObjects.Gll;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Matches any input item that isn't the end of input. Consumes exactly one input item and
/// returns it.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record AnyParser<T>(
    string Name = ""
) : IParser<T, T>, IGllParser<T, T>
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

    public void Parse(IState<T> state, IResultPromise results)
    {
        if (state.Input.IsAtEnd)
        {
            results.AddFailure(state, "Expected any but found End");
            results.IsComplete = true;
            return;
        }

        var value = state.Input.GetNext();
        results.Add(state.Advance().Success(value));
        results.IsComplete = true;
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => this with { Name = name };
}
