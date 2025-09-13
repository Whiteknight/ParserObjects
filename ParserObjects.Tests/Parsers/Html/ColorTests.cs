using static ParserObjects.Parsers.Html;

namespace ParserObjects.Tests.Parsers.Html;

public class ColorTests
{
    [TestCase("#FFFFFF", 255, 255, 255)]
    [TestCase("#000000", 0, 0, 0)]
    [TestCase("#C0C0C0", 192, 192, 192)]
    [TestCase("#FF5733", 255, 87, 51)]
    public void Test(string input, int r, int g, int b)
    {
        var parser = Color();
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.R.Should().Be((byte)r);
        result.Value.G.Should().Be((byte)g);
        result.Value.B.Should().Be((byte)b);
    }
}
