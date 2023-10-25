using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

public sealed class MatchStringPatternParser : IParser<char, string>
{
    public MatchStringPatternParser(string pattern, bool caseInsensitive, string name = "")
    {
        Pattern = pattern;
        CaseInsensitive = caseInsensitive;
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();
    public string Pattern { get; }
    public bool CaseInsensitive { get; }
    public string Name { get; }

    public IResult<string> Parse(IParseState<char> state)
    {
        Assert.ArgumentNotNull(state);
        Debug.Assert(Pattern.Length > 0, "We shouldn't have empty patterns here");

        var checkpoint = state.Input.Checkpoint();

        // If the input is an ICharSequence we can optimize with .GetString
        if (state.Input is ICharSequence charSequence)
        {
            var s = charSequence.GetString(Pattern.Length);
            if (s.Length < Pattern.Length || !Pattern.Equals(s, CaseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
            {
                checkpoint.Rewind();
                return state.Fail(this, "Pattern does not match");
            }

            return state.Success(this, s, s.Length);
        }

        return CaseInsensitive ? ParseCaseInsensitive(state, checkpoint) : ParseCaseSensitive(state, checkpoint);
    }

    private IResult<string> ParseCaseSensitive(IParseState<char> state, SequenceCheckpoint checkpoint)
    {
        if (Pattern.Length == 1)
        {
            var next = state.Input.Peek();
            if (next != Pattern[0])
                return state.Fail(this, "Item does not match");
            return state.Success(this, Pattern, 1);
        }

        for (var i = 0; i < Pattern.Length; i++)
        {
            var c = state.Input.GetNext();
            if (c == Pattern[i])
                continue;

            checkpoint.Rewind();
            return state.Fail(this, $"Item does not match at position {i}");
        }

        return state.Success(this, Pattern, Pattern.Length);
    }

    private IResult<string> ParseCaseInsensitive(IParseState<char> state, SequenceCheckpoint checkpoint)
    {
        if (Pattern.Length == 1)
        {
            var next = state.Input.Peek();
            if (!CharMethods.EqualsCaseInsensitive(next, Pattern[0]))
                return state.Fail(this, "Item does not match");
            return state.Success(this, new string(next, 1), 1);
        }

        var buffer = new char[Pattern.Length];
        for (var i = 0; i < Pattern.Length; i++)
        {
            var c = state.Input.GetNext();
            buffer[i] = c;
            if (CharMethods.EqualsCaseInsensitive(c, Pattern[i]))
                continue;

            checkpoint.Rewind();
            return state.Fail(this, $"Item does not match at position {i}");
        }

        return state.Success(this, new string(buffer, 0, Pattern.Length), Pattern.Length);
    }

    IResult IParser<char>.Parse(IParseState<char> state) => Parse(state);

    public bool Match(IParseState<char> state)
    {
        if (Pattern.Length == 0)
            return true;

        Func<char, char, bool> test = CaseInsensitive ? CharMethods.EqualsCaseInsensitive : CharMethods.EqualsCaseSensitive;
        var checkpoint = state.Input.Checkpoint();
        for (int i = 0; i < Pattern.Length; i++)
        {
            if (!test(Pattern[i], state.Input.GetNext()))
            {
                checkpoint.Rewind();
                return false;
            }
        }

        return true;
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("MatchPattern", Name, Id);

    public INamed SetName(string name) => new MatchStringPatternParser(Pattern, CaseInsensitive, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
