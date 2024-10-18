using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

internal class HexadecimalIntegerTests
{
    [TestCase("0x0", 0U)]
    [TestCase("0x1", 1U)]
    [TestCase("0x2", 2U)]
    [TestCase("0x3", 3U)]
    [TestCase("0x4", 4U)]
    [TestCase("0x5", 5U)]
    [TestCase("0x6", 6U)]
    [TestCase("0x7", 7U)]
    [TestCase("0x8", 8U)]
    [TestCase("0x9", 9U)]
    [TestCase("0xA", 10U)]
    [TestCase("0xB", 11U)]
    [TestCase("0xC", 12U)]
    [TestCase("0xD", 13U)]
    [TestCase("0xE", 14U)]
    [TestCase("0xF", 15U)]
    [TestCase("0xa", 10U)]
    [TestCase("0xb", 11U)]
    [TestCase("0xc", 12U)]
    [TestCase("0xd", 13U)]
    [TestCase("0xe", 14U)]
    [TestCase("0xf", 15U)]
    [TestCase("0xAB12", 0xAB12U)]
    public void Parse_Test(string test, uint value)
    {
        var parser = HexadecimalInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [TestCase("0xG")]
    [TestCase("0x_")]
    [TestCase("0x-5")]
    public void Parse_Fail(string test)
    {
        var parser = HexadecimalInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
