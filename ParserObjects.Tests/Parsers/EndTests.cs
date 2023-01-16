using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class EndTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = End();
        parser.Match("").Should().BeTrue();
        parser.Match("x").Should().BeFalse();
    }

    [Test]
    public void GetChildren_Test()
    {
        var parser = End();
        parser.GetChildren().Count().Should().Be(0);
    }

    [Test]
    public void ToBnf_Test()
    {
        var parser = End().Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := END");
    }
}
