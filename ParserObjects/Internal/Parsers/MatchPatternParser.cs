using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Internal.Visitors;

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

    public Result<IReadOnlyList<T>> Parse(IParseState<T> state)
    {
        Assert.NotNull(state);
        Debug.Assert(Pattern.Count > 0);

        // If the pattern has exactly one item in it, check for equality without a loop
        // or allocating a buffer
        if (Pattern.Count == 1)
        {
            var next = state.Input.Peek();
            return next is null || !next.Equals(Pattern[0])
                ? Result.Fail(this, "Item does not match")
                : Result.Ok(this, (IReadOnlyList<T>)[state.Input.GetNext()], 1);
        }

        var checkpoint = state.Input.Checkpoint();
        var buffer = new T[Pattern.Count];
        for (var i = 0; i < Pattern.Count; i++)
        {
            var c = state.Input.GetNext();
            if (c is null)
            {
                checkpoint.Rewind();
                return Result.Fail(this, $"Item does not match at position {i}");
            }

            buffer[i] = c;
            if (c.Equals(Pattern[i]))
                continue;

            checkpoint.Rewind();
            return Result.Fail(this, $"Item does not match at position {i}");
        }

        return Result.Ok(this, (IReadOnlyList<T>)buffer, Pattern.Count);
    }

    Result<object> IParser<T>.Parse(IParseState<T> state) => Parse(state).AsObject();

    public bool Match(IParseState<T> state)
    {
        Debug.Assert(Pattern.Count > 0);

        var checkpoint = state.Input.Checkpoint();
        for (int i = 0; i < Pattern.Count; i++)
        {
            if (!Equals(Pattern[i], state.Input.GetNext()))
            {
                checkpoint.Rewind();
                return false;
            }
        }

        return true;
    }

    public IEnumerable<IParser> GetChildren() => [];

    public override string ToString() => DefaultStringifier.ToString("MatchPattern", Name, Id);

    public INamed SetName(string name) => this with { Name = name };

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
