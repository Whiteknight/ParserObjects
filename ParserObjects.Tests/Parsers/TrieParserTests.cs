using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using ParserObjects.Utility;
using static ParserObjects.Parsers.ParserMethods<char>;

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

        [Test]
        public void AddMany_ToParser_Operators()
        {
            var trie = new InsertOnlyTrie<char, string>();
            trie.AddMany("=", "==", ">=", "<=", "<", ">");
            var target = trie.ToParser();

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

        [Test]
        public void ParseUntyped_Operators()
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

            target.ParseUntyped(input).Value.Should().Be("==");
            target.ParseUntyped(input).Value.Should().Be("=");
            target.ParseUntyped(input).Value.Should().Be(">=");
            target.ParseUntyped(input).Value.Should().Be("<=");
            target.ParseUntyped(input).Value.Should().Be(">");
            target.ParseUntyped(input).Value.Should().Be("<");
            target.ParseUntyped(input).Value.Should().Be("<=");
            target.ParseUntyped(input).Value.Should().Be("=");
        }

        [Test]
        public void Parse_Operators_Fail()
        {
            var trie = new InsertOnlyTrie<char, string>();
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
            var target = new TrieParser<char, string>(trie);

            var input = new StringCharacterSequence("X===>=<=><<==");

            target.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void Trie_Action_Operators()
        {
            var target = Trie<string>(trie => trie
                .Add("=", "=")
                .Add("==", "==")
                .Add(">=", ">=")
                .Add("<=", "<=")
                .Add("<", "<")
                .Add(">", ">")
            );

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

        [Test]
        public void Parse_Operators_Method()
        {
            var trie = new InsertOnlyTrie<char, string>();
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
            var target = Trie(trie);

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

        [Test]
        public void MatchAny_Parse_Operators()
        {
            var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });

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

        [Test]
        public void GetChildren_Test()
        {
            var trie = new InsertOnlyTrie<char, string>();
            var target = new TrieParser<char, string>(trie);
            target.GetChildren().Count().Should().Be(0);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var trie = new InsertOnlyTrie<char, string>();
            var target = new TrieParser<char, string>(trie);
            target.ReplaceChild(Empty(), Produce(() => "")).Should().BeSameAs(target);
        }
    }
}
