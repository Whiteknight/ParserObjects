using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Internal.Visitors;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// A MatchPatternParser variant optimized to match characters against a string pattern and return
/// results as a string.
/// </summary>
public sealed class MatchStringPatternParser : IParser<char, string>
{
    public MatchStringPatternParser(string pattern, IEqualityComparer<char> comparer, string name = "")
    {
        Pattern = pattern;
        Comparer = comparer;
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Pattern { get; }

    public IEqualityComparer<char> Comparer { get; }

    public string Name { get; }

    public Result<string> Parse(IParseState<char> state)
    {
        NotNull(state);
        Debug.Assert(Pattern.Length > 0);

        var checkpoint = state.Input.Checkpoint();

        // If the input is an ICharSequence we can optimize with .GetString
        if (state.Input is ICharSequence charSequence)
        {
            var s = charSequence.GetString(Pattern.Length);
            if (!Compare(s))
            {
                checkpoint.Rewind();
                return Result.Fail(this, "Pattern does not match");
            }

            return Result.Ok(this, s, s.Length);
        }

        return Parse(state, Comparer, checkpoint);
    }

    private Result<string> Parse(IParseState<char> state, IEqualityComparer<char> comparer, SequenceCheckpoint checkpoint)
    {
        if (Pattern.Length == 1)
        {
            var next = state.Input.Peek();
            return comparer.Equals(next, Pattern[0])
                ? Result.Ok(this, Pattern, 1)
                : Result.Fail(this, "Item does not match");
        }

        for (var i = 0; i < Pattern.Length; i++)
        {
            var c = state.Input.GetNext();
            if (comparer.Equals(c, Pattern[i]))
                continue;

            checkpoint.Rewind();
            return Result.Fail(this, $"Item does not match at position {i}");
        }

        return Result.Ok(this, Pattern, Pattern.Length);
    }

    private bool Compare(string input)
    {
        if (input.Length != Pattern.Length)
            return false;

        for (int i = 0; i < input.Length; i++)
        {
            if (!Comparer.Equals(input[i], Pattern[i]))
                return false;
        }

        return true;
    }

    Result<object> IParser<char>.Parse(IParseState<char> state) => Parse(state).AsObject();

    public bool Match(IParseState<char> state)
    {
        if (Pattern.Length == 0)
            return true;

        var checkpoint = state.Input.Checkpoint();
        for (int i = 0; i < Pattern.Length; i++)
        {
            if (!Comparer.Equals(Pattern[i], state.Input.GetNext()))
            {
                checkpoint.Rewind();
                return false;
            }
        }

        return true;
    }

    public IEnumerable<IParser> GetChildren() => [];

    public override string ToString() => DefaultStringifier.ToString("MatchPattern", Name, Id);

    public INamed SetName(string name) => new MatchStringPatternParser(Pattern, Comparer, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
