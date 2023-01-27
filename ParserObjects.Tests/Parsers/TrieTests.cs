using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class TrieTests
{
    [Test]
    public void Parse_Operators()
    {
        var target = Trie<string>(trie =>
        {
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
        });

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
        var target = Trie<string>(trie =>
        {
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
        });

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
        void Populate(InsertableTrie<char, string> trie)
        {
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add("===", "===");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
        }

        var multiTrieParser = TrieMulti<string>(Populate);
        var singleTrieParser = Trie<string>(Populate);

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
        var target = Trie<string>(trie =>
        {
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
        });

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

    [TestCase("=")]
    [TestCase("==")]
    [TestCase(">=")]
    [TestCase("<=")]
    [TestCase("<")]
    [TestCase(">")]
    public void Match_Operators(string oper)
    {
        var target = Trie<string>(trie =>
        {
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
        });

        var input = FromString(oper);

        target.Match(input).Should().BeTrue();
        input.Consumed.Should().Be(oper.Length);
    }

    [Test]
    public void Match_Operators_Fail()
    {
        var target = Trie<string>(trie =>
        {
            trie.Add("=", "=");
            trie.Add("==", "==");
            trie.Add(">=", ">=");
            trie.Add("<=", "<=");
            trie.Add("<", "<");
            trie.Add(">", ">");
        });

        var input = FromString("X");
        target.Match(input).Should().BeFalse();
        input.Consumed.Should().Be(0);
    }

    [Test]
    public void GetChildren_Test()
    {
        var target = Trie<char>(_ => { });
        target.GetChildren().Count().Should().Be(0);
    }

    [Test]
    public void ToBnf_Test()
    {
        var parser = Trie<string>(trie => trie
                .Add("abc")
                .Add("abd")
                .Add("xyz")
            )
            .Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := ('a' 'b' 'c') | ('a' 'b' 'd') | ('x' 'y' 'z')");
    }

    [Test]
    public void ToBnf_Empty()
    {
        var parser = Trie<string>(trie => { }).Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := FAIL");
    }
}
