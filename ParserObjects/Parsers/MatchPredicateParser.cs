using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Returns the next input item if it satisfies a predicate, failure otherwise. Notice that
/// the end sentinel will be made available to the predicate and may match.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record MatchPredicateParser<T>(
    Func<T, bool> Predicate,
    string Name = ""
) : IParser<T, T>
{
    public IResult<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var location = state.Input.CurrentLocation;
        var startConsumed = state.Input.Consumed;

        var next = state.Input.Peek();
        if (next == null || !Predicate(next))
            return state.Fail(this, "Next item does not match the predicate");

        state.Input.GetNext();
        return state.Success(this, next, state.Input.Consumed - startConsumed, location);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => this with { Name = name };
}
