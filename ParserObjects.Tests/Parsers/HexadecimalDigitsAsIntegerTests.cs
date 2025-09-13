using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class HexadecimalDigitsAsIntegerTests
{
    [TestCase("00", 0)]
    [TestCase("FF", 255)]
    [TestCase("80", 128)]
    public void Parse_Test(string input, int expected)
    {
        var target = HexadecimalDigitsAsInteger(2, 4);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }
}
