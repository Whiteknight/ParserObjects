using System.Linq;
using ParserObjects.Utility;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.SequenceMethods;

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
            IParser<char, string> target = Trie(trie);

            var input = FromString("===>=<=><<==");

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
        public void Parse_Operators_Fail()
        {
            var trie = new InsertOnlyTrie<char, string>();
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
            IParser<char, string> target = Trie(trie);

            var input = FromString("X===>=<=><<==");

            target.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void Single_Action_Operators()
        {
            var target = Trie<string>(trie => trie
                .Add("=", "=")
                .Add("==", "==")
                .Add(">=", ">=")
                .Add("<=", "<=")
                .Add("<", "<")
                .Add(">", ">")
            );

            var input = FromString("===>=<=><<==");

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
        public void Multi_Action_Operators()
        {
            var target = TrieMulti<string>(trie => trie
                .Add("=", "=")
                .Add("==", "==")
                .Add("===", "===")
                .Add(">=", ">=")
                .Add("<=", "<=")
                .Add("<", "<")
                .Add(">", ">")
            );

            var input = FromString("===>=<=><<==");

            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results[0].Value.Should().Be("=");
            result.Results[0].Consumed.Should().Be(1);
            result.Results[1].Value.Should().Be("==");
            result.Results[1].Consumed.Should().Be(2);
            result.Results[2].Value.Should().Be("===");
            result.Results[2].Consumed.Should().Be(3);
        }

        [Test]
        public void Multi_Action_Continue()
        {
            var trie = new InsertOnlyTrie<char, string>();
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add("===", "===");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");

            var multiTrieParser = TrieMulti(trie);
            var singleTrieParser = Trie(trie);

            var target = multiTrieParser.ContinueWith(left => Rule(
                left,
                singleTrieParser,
                (l, r) => $"{l} {r}"
            ));
            var input = FromString("===>=<=><<==");

            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(3);
            result.Results[0].Value.Should().Be("= ==");
            result.Results[1].Value.Should().Be("== =");
            result.Results[2].Value.Should().Be("=== >=");
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

            var input = FromString("===>=<=><<==");

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
        public void Parse_Consumed()
        {
            var target = Trie<string>(trie => trie
                .Add("=", "=")
                .Add("==", "==")
                .Add(">=", ">=")
                .Add("<=", "<=")
                .Add("<", "<")
                .Add(">", ">")
            );

            var input = FromString("===X");

            var result = target.Parse(input);
            result.Value.Should().Be("==");
            result.Consumed.Should().Be(2);

            result = target.Parse(input);
            result.Value.Should().Be("=");
            result.Consumed.Should().Be(1);

            result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_IsAtEnd()
        {
            var target = Trie<string>(trie => trie
                .Add("\0\0\0", "ok")
            );

            var input = FromString("\0");

            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var trie = new InsertOnlyTrie<char, string>();
            var target = Trie(trie);
            target.GetChildren().Count().Should().Be(0);
        }
    }
}
