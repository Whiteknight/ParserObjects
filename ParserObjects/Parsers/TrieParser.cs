using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser which wraps an ITrie to be able to return elements which match one of several possible
    /// input patterns. Adaptor from ITrie to IParser.
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

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            var (success, value, location) = Trie.Get(t.Input);
            if (!success)
                return t.Fail(this, "Trie did not contain matching value");
            return t.Success(this, value, location);
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
