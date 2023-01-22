using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

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
    public void Parse_Fail_NoConsumed()
    {
        var target = NonGreedyList(
            Produce(() => 'X'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Parse("aaaab");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_Minimum()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            minimum: 3
        );

        var result = target.Parse("aaaab");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("(aaa)(ab)");
        result.Consumed.Should().Be(5);
    }

    [Test]
    public void Parse_Minimum_Fail0()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            minimum: 3
        );

        var result = target.Parse("Xab");
        result.Success.Should().BeFalse();
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
    public void Parse_SeparatorMissing()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            MatchChar(','),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Parse("a,aa,ab");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_SeparatorMissing_BelowMinimum()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            MatchChar(','),
            l => Rule(
                l,
                MatchChar('a'),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            minimum: 3
        );

        var result = target.Parse("a,aa,ab");
        result.Success.Should().BeFalse();
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
    public void Parse_Fail_BelowMinimum()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("B"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            minimum: 3
        );

        var result = target.Parse("aaB");
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
    public void Match_Test()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var input = FromString("aaaab");
        var result = target.Match(input);
        result.Should().BeTrue();
        input.Consumed.Should().Be(5);
    }

    [Test]
    public void Match_Minimum()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            minimum: 3
        );

        var input = FromString("aaaab");
        var result = target.Match(input);
        result.Should().BeTrue();
        input.Consumed.Should().Be(5);
    }

    [Test]
    public void Match_Separator()
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

        var input = FromString("a,a,a,ab");
        var result = target.Match(input);
        result.Should().BeTrue();
        input.Consumed.Should().Be(8);
    }

    [Test]
    public void Match_SeparatorMissing()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            MatchChar(','),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Match("a,aa,ab");
        result.Should().BeFalse();
    }

    [Test]
    public void Match_NoItems()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var input = FromString("ab");
        var result = target.Match(input);
        result.Should().BeTrue();
        input.Consumed.Should().Be(2);
    }

    [Test]
    public void Match_Fail_NoItemsMinimum()
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

        var result = target.Match("ab");
        result.Should().BeFalse();
    }

    [Test]
    public void Match_Fail_BelowMinimum()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("B"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            ),
            minimum: 3
        );

        var result = target.Match("aaB");
        result.Should().BeFalse();
    }

    [Test]
    public void Match_Fail_MoreThanMaximum()
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
        var result = target.Match("aaaab");
        result.Should().BeFalse();
    }

    [Test]
    public void Match_Fail_NoContinuation()
    {
        var target = NonGreedyList(
            MatchChar('a'),
            l => Rule(
                l,
                CharacterString("ab"),
                (x, y) => $"({new string(x.ToArray())})({y})"
            )
        );

        var result = target.Match("aaaaX");
        result.Should().BeFalse();
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

    [Test]
    public void ToBnf_Test()
    {
        var parser = NonGreedyList(MatchChar('a'), _ => MatchChar('Z')).Named("SUT");
        parser.ToBnf().Should().Contain("SUT := 'a'*? 'Z'");
    }

    [TestCase(0, 1, "'a'?")]
    [TestCase(4, 4, "'a'{4}?")]
    [TestCase(0, 10, "'a'{0, 10}?")]
    [TestCase(0, null, "'a'*?")]
    [TestCase(1, null, "'a'+?")]
    [TestCase(3, null, "'a'{3,}?")]
    public void ToBnf_MinMax(int min, int? max, string pattern)
    {
        var parser = NonGreedyList(MatchChar('a'), _ => MatchChar('Z'), minimum: min, maximum: max).Named("SUT");
        parser.ToBnf().Should().Contain($"SUT := {pattern} 'Z'");
    }

    [TestCase(0, 1, "'a' ('b' 'a')?")]
    [TestCase(4, 4, "'a' ('b' 'a'){4}?")]
    [TestCase(0, 10, "'a' ('b' 'a'){0, 10}?")]
    [TestCase(0, null, "'a' ('b' 'a')*?")]
    [TestCase(1, null, "'a' ('b' 'a')+?")]
    [TestCase(3, null, "'a' ('b' 'a'){3,}?")]
    public void ToBnf_MinMax_Separator(int min, int? max, string pattern)
    {
        var parser = NonGreedyList(MatchChar('a'), MatchChar('b'), _ => MatchChar('Z'), minimum: min, maximum: max).Named("SUT");
        parser.ToBnf().Should().Contain($"SUT := {pattern} 'Z'");
    }
}
