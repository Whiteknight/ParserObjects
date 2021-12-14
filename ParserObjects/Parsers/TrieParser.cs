using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Uses a Trie to match the longest pattern from among available options.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed record TrieParser<TInput, TOutput>(
    IReadOnlyTrie<TInput, TOutput> Trie,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        var result = Trie.Get(state.Input);
        return state.Result(this, result);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => this with { Name = name };
}
