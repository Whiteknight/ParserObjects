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
public sealed class MatchPredicateParser<T> : IParser<T, T>
{
    private readonly Func<T, bool> _predicate;

    public MatchPredicateParser(Func<T, bool> predicate, string name = "")
    {
        Assert.ArgumentNotNull(predicate, nameof(predicate));
        _predicate = predicate;
        Name = name;
    }

    public string Name { get; }

    public IResult<T> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var location = state.Input.CurrentLocation;
        var startConsumed = state.Input.Consumed;

        var next = state.Input.Peek();
        if (next == null || !_predicate(next))
            return state.Fail(this, "Next item does not match the predicate");

        state.Input.GetNext();
        return state.Success(this, next, state.Input.Consumed - startConsumed, location);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => new MatchPredicateParser<T>(_predicate, name);
}
