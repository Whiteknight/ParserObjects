using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Regexes;
using ParserObjects.Internal.Utility;
using ParserObjects.Regexes;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Uses limited Regular Expression syntax to match a pattern of characters.
/// </summary>
public sealed class RegexParser : IParser<char, string>
{
    public RegexParser(Regex regex, string describe, string? name = null)
    {
        Assert.ArgumentNotNull(regex, nameof(regex));
        Regex = regex;
        Name = name ?? $"/{describe}/";
        Pattern = describe;
    }

    public Regex Regex { get; }

    public string Pattern { get; }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public IResult<string> Parse(IParseState<char> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCp = state.Input.Checkpoint();
        var result = Engine.GetMatch(state.Input, Regex);
        if (!result.Success)
        {
            startCp.Rewind();
            return state.Fail(this, result.ErrorMessage!);
        }

        return state.Success(this, result.Value!, result.Consumed, result.Location, new[]
        {
            new RegexMatch(result.Value!, result.Captures!)
        });
    }

    IResult IParser<char>.Parse(IParseState<char> state) => Parse(state);

    public bool Match(IParseState<char> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
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
}
