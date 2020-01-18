using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects.Tests.Parsers
{
    public class TrieParserTests
    {
        [Test]
        public void Parse_Operators()
        {
            var trie = new InsertOnlyTrie<char, string>();
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
            var target = new TrieParser<char, string>(trie);

            var input = new StringCharacterSequence("===>=<=><<==");

            target.Parse(input).Value.Should().Be("==");
            target.Parse(input).Value.Should().Be("=");
            target.Parse(input).Value.Should().Be(">=");
            target.Parse(input).Value.Should().Be("<=");
            target.Parse(input).Value.Should().Be(">");
            target.Parse(input).Value.Should().Be("<");
            target.Parse(input).Value.Should().Be("<=");
            target.Parse(input).Value.Should().Be("=");
        }
    }
}
