using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class EmptyTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Empty();
        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Test]
    public void Parse_End()
    {
        var parser = Empty();
        var input = FromString("");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Test]
    public void GetChildren_Test()
    {
        var parser = Empty();
        parser.GetChildren().Count().Should().Be(0);
    }

    [Test]
    public void ToBnf_Test()
    {
        var parser = Empty().Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := ()");
    }
}
