using static ParserObjects.Parsers.Html;

namespace ParserObjects.Tests.Parsers.Html;

public class ColorCodeTests
{
    [TestCase("#FFFFFF")]
    [TestCase("#000000")]
    [TestCase("#C0C0C0")]
    public void Test(string input)
    {
        var parser = ColorCode();
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(input);
    }
}
