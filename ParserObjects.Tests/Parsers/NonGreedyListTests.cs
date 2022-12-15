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
        result.Consumed.Should().Be(5);
    }

    [Test]
    public void Parse_Separator()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            MatchChar(','),
            l => Rule(
                l,
                CharacterString(",ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Parse("a,a,a,ab");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("(aaa)(,ab)");
        result.Consumed.Should().Be(8);
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
        result.Consumed.Should().Be(2);
    }

    [Test]
    public void Parse_Fail_NoItemsMinimum()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            minimum: 2
        );

        var result = target.Parse("ab");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_Fail_MoreThanMaximum()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            maximum: 2
        );

        // Can parse a maximum of 2 "a", but then has "aab" which does not match
        // the continuation parser
        var result = target.Parse("aaaab");
        result.Success.Should().BeFalse();
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
        children.Count.Should().Be(3);
        children.Should().Contain(p => p.Name == "InnerRule");
    }
}
