using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class PeekTests
{
    [Test]
    public void Parse_Test()
    {
        var target = Peek();
        var input = FromString("abc");
        target.Parse(input).Value.Should().Be('a');
        target.Parse(input).Value.Should().Be('a');
        target.Parse(input).Value.Should().Be('a');
        input.Consumed.Should().Be(0);
    }

    [Test]
    public void Match_Test()
    {
        var target = Peek();
        var input = FromString("abc");
        target.Match(input).Should().BeTrue();
        target.Match(input).Should().BeTrue();
        target.Match(input).Should().BeTrue();
        input.Consumed.Should().Be(0);
    }

    [Test]
    public void GetChildren_Test()
    {
        var target = Peek();
        target.GetChildren().Count().Should().Be(0);
    }
}
