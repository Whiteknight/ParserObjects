using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Regexes;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Uses limited Regular Expression syntax to match a pattern of characters.
/// </summary>
public sealed class RegexParser : IParser<char, string>
{
    private readonly int _maxItems;

    public RegexParser(Regex regex, string describe, int maxItems = 0, string? name = null)
    {
        Assert.ArgumentNotNull(regex, nameof(regex));
        Regex = regex;
        Name = name ?? $"/{describe}/";
        Pattern = describe;
        _maxItems = maxItems;
    }

    public Regex Regex { get; }

    public string Pattern { get; }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public IResult<string> Parse(IParseState<char> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCp = state.Input.Checkpoint();
        var result = Engine.GetMatch(state.Input, Regex, _maxItems);
        if (!result.Success)
            startCp.Rewind();
        return state.Result(this, result);
    }

    IResult IParser<char>.Parse(IParseState<char> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Regex", Name, Id);

    public INamed SetName(string name) => new RegexParser(Regex, Pattern, _maxItems, name);
}
