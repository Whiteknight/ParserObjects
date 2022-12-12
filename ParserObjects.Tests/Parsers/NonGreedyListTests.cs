using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class NonGreedyListTests
{
    [Test]
    public void Parse_Test()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Parse("aaaab");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("(aaa)(ab)");
    }

    [Test]
    public void Parse_NoItems()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Parse("ab");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("()(ab)");
    }

    [Test]
    public void Parse_Fail_NoContinuation()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Parse("aaaaX");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void GetChildren_Test()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ).Named("InnerRule")
        );

        var children = target.GetChildren().ToList();
        children.Count.Should().Be(2);
        children.Should().Contain(p => p.Name == "InnerRule");
    }
}
