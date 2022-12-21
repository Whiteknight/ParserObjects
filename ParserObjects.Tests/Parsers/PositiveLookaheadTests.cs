using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class PositiveLookaheadTests
{
    [Test]
    public void Parse_Test()
    {
        var nextParser = Any();
        var parser = PositiveLookahead(nextParser);

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Parse_Fail()
    {
        var failParser = Fail<char>();
        var parser = PositiveLookahead(failParser);

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Match_Test()
    {
        var nextParser = Any();
        var parser = PositiveLookahead(nextParser);

        var input = FromString("abc");
        var result = parser.Match(input);
        result.Should().BeTrue();
        input.Consumed.Should().Be(0);
    }

    [Test]
    public void Match_Fail()
    {
        var failParser = Fail<char>();
        var parser = PositiveLookahead(failParser);

        var input = FromString("abc");
        var result = parser.Match(input);
        result.Should().BeFalse();
        input.Consumed.Should().Be(0);
    }

    [Test]
    public void GetChildren_Test()
    {
        var failParser = Fail<char>();
        var parser = PositiveLookahead(failParser);
        var result = parser.GetChildren().ToList();
        result.Count.Should().Be(1);
        result[0].Should().BeSameAs(failParser);
    }
}
