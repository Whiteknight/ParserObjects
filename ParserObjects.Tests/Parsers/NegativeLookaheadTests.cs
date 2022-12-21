using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class NegativeLookaheadTests
{
    [Test]
    public void Parse_Test()
    {
        var failParser = Fail<char>();
        var parser = NegativeLookahead(failParser);

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().Be(true);
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Parse_Fail()
    {
        var nextParser = Any();
        var parser = NegativeLookahead(nextParser);

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().Be(false);
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Match_Test()
    {
        var failParser = Fail<char>();
        var parser = NegativeLookahead(failParser);

        var input = FromString("abc");
        var result = parser.Match(input);
        result.Should().Be(true);
    }

    [Test]
    public void Match_Fail()
    {
        var nextParser = Any();
        var parser = NegativeLookahead(nextParser);

        var input = FromString("abc");
        var result = parser.Match(input);
        result.Should().Be(false);
    }

    [Test]
    public void GetChildren_Test()
    {
        var failParser = Fail<char>();
        var parser = NegativeLookahead(failParser);
        var result = parser.GetChildren().ToList();
        result.Count.Should().Be(1);
        result[0].Should().BeSameAs(failParser);
    }
}
