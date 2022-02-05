using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Uses a Trie to match the longest pattern from among available options.
/// </summary>
public sealed record TrieParser<TInput, TOutput>(
    IReadOnlyTrie<TInput, TOutput> Trie,
    string Name = ""
) : IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        var result = Trie.Get(state.Input);
        return state.Result(this, result);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    IMultiResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        var startCheckpoint = state.Input.Checkpoint();
        var results = Trie.GetMany(state.Input);
        return new MultiResult<TOutput>(this, startCheckpoint.Location, startCheckpoint, results);
    }

    IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => ((IMultiParser<TInput, TOutput>)this).Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Trie", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
