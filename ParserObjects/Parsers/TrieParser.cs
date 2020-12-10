﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Uses an ITrie to match the longest pattern.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class TrieParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public IReadOnlyTrie<TInput, TOutput> Trie { get; }

        public TrieParser(IReadOnlyTrie<TInput, TOutput> trie)
        {
            Assert.ArgumentNotNull(trie, nameof(trie));
            Trie = trie;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var result = Trie.Get(state.Input);
            return state.Result(this, result);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
