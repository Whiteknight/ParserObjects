using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.C;

internal class HexadecimalIntegerTests
{
    [TestCase("0x0", 0)]
    [TestCase("0x1", 1)]
    [TestCase("0x2", 2)]
    [TestCase("0x3", 3)]
    [TestCase("0x4", 4)]
    [TestCase("0x5", 5)]
    [TestCase("0x6", 6)]
    [TestCase("0x7", 7)]
    [TestCase("0x8", 8)]
    [TestCase("0x9", 9)]
    [TestCase("0xA", 10)]
    [TestCase("0xB", 11)]
    [TestCase("0xC", 12)]
    [TestCase("0xD", 13)]
    [TestCase("0xE", 14)]
    [TestCase("0xF", 15)]
    [TestCase("0xa", 10)]
    [TestCase("0xb", 11)]
    [TestCase("0xc", 12)]
    [TestCase("0xd", 13)]
    [TestCase("0xe", 14)]
    [TestCase("0xf", 15)]
    [TestCase("0xAB12", 0xAB12)]
    public void Parse_Test(string test, int value)
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
