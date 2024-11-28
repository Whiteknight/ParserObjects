using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Regexes;
using ParserObjects.Internal.Visitors;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Uses limited Regular Expression syntax to match a pattern of characters.
/// </summary>
public sealed class RegexParser : IParser<char, string>, IParser<char, RegexMatch>
{
    /* Delegates to the Regex struct to hold the pattern definition.
     * Delegates to the Engine to test the pattern against the input.
     */

    public RegexParser(Regex regex, string describe, string? name = null)
    {
        Assert.ArgumentNotNull(regex);
        Regex = regex;
        Name = name ?? $"/{describe}/";
        Pattern = describe;
    }

    public Regex Regex { get; }

    public string Pattern { get; }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    Result<RegexMatch> IParser<char, RegexMatch>.Parse(IParseState<char> state)
    {
        Assert.ArgumentNotNull(state);
        var startCp = state.Input.Checkpoint();
        var result = Engine.GetMatch(state.Input, Regex);
        if (!result.Success)
        {
            startCp.Rewind();
            return Result.Fail<RegexMatch>(this, result.ErrorMessage!);
        }

        return Result.Ok(this, result.Match!, result.Consumed, new ResultData(result.Match!));
    }

    Result<string> IParser<char, string>.Parse(IParseState<char> state)
    {
        Assert.ArgumentNotNull(state);
        var startCp = state.Input.Checkpoint();
        var result = Engine.GetMatch(state.Input, Regex);
        if (!result.Success)
        {
            startCp.Rewind();
            return Result.Fail<string>(this, result.ErrorMessage!);
        }

        return Result.Ok(this, result.Value!, result.Consumed, new ResultData(result.Match!));
    }

    Result<object> IParser<char>.Parse(IParseState<char> state) => ((IParser<char, RegexMatch>)this).Parse(state).AsObject();

    public bool Match(IParseState<char> state)
    {
        Assert.ArgumentNotNull(state);
        var startCp = state.Input.Checkpoint();
        var result = Engine.TestMatch(state.Input, Regex);
        if (!result)
        {
            startCp.Rewind();
            return false;
        }

        return true;
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Regex", Name, Id);

    public INamed SetName(string name) => new RegexParser(Regex, Pattern, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IRegexPartialVisitor<TState>>()?.Accept(this, state);
    }
}
