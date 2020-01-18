using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class TrieParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly ITrie<TInput, TOutput> _trie;

        public TrieParser(ITrie<TInput, TOutput> trie)
        {
            _trie = trie;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t) => _trie.Get(t);

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => _trie.Get(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
