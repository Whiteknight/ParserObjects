using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Gll;
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
) : IParser<T, T>, IGllParser<T, T>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

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

    public void Parse(IState<T> state, IResultPromise results)
    {
        if (state.Input.IsAtEnd)
        {
            results.Add(state.Failure($"Expected matching value but found End"));
            results.IsComplete = true;
            return;
        }

        var value = state.Input.Peek();
        if (!Predicate(value))
        {
            results.Add(state.Failure($"Expected matching value but found {value}"));
            results.IsComplete = true;
            return;
        }

        state.Input.GetNext();

        results.Add(state.Advance().Success(value));
        results.IsComplete = true;
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => this with { Name = name };
}
