using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Given a literal sequence of values, pull values off the input sequence to match. If the
/// entire series matches, return it. Otherwise return failure.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record MatchPatternParser<T>(
    IReadOnlyList<T> Pattern,
    string Name = ""
) : IParser<T, IReadOnlyList<T>>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<IReadOnlyList<T>> Parse(IParseState<T> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var location = state.Input.CurrentLocation;

        // If the pattern is empty, return success.
        if (Pattern.Count == 0)
        {
            state.Log(this, "Pattern has 0 items in it, this is functionally equivalent to Empty() ");
            return state.Success(this, Array.Empty<T>(), 0, location);
        }

        // If the pattern has exactly one item in it, check for equality without a loop
        // or allocating a buffer
        if (Pattern.Count == 1)
        {
            var next = state.Input.Peek();
            if (next == null || !next.Equals(Pattern[0]))
                return state.Fail(this, "Item does not match");
            return state.Success(this, new[] { state.Input.GetNext() }, 1, location);
        }

        var checkpoint = state.Input.Checkpoint();
        var buffer = new T[Pattern.Count];
        for (var i = 0; i < Pattern.Count; i++)
        {
            var c = state.Input.GetNext();
            if (c == null)
            {
                checkpoint.Rewind();
                return state.Fail(this, $"Item does not match at position {i}");
            }

            buffer[i] = c;
            if (c.Equals(Pattern[i]))
                continue;

            checkpoint.Rewind();
            return state.Fail(this, $"Item does not match at position {i}");
        }

        return state.Success(this, buffer, Pattern.Count, location);
    }

    IResult IParser<T>.Parse(IParseState<T> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("MatchPattern", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
