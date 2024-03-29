﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Tries;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Uses a Trie to match the longest pattern from among available options.
/// </summary>
public sealed record TrieParser<TInput, TOutput>(
    ReadableTrie<TInput, TOutput> Trie,
    string Name = ""
) : IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
{
    /* This parser delegates to the ReadableTrie struct, which in turn delegates to the Trie/Node
     * class. All the parsing/matching logic is located in Node.
     */

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        var result = Trie.Get(state.Input);
        return state.Result(this, result);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state) => Trie.CanGet(state.Input);

    IMultiResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        var startCheckpoint = state.Input.Checkpoint();
        var results = Trie.GetMany(state.Input);
        return new MultiResult<TOutput>(this, startCheckpoint, results);
    }

    IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => ((IMultiParser<TInput, TOutput>)this).Parse(state);

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Trie", Name, Id);

    public INamed SetName(string name) => this with { Name = name };

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IMatchPartialVisitor<TState>>()?.Accept(this, state);
    }
}
