using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class UnsignedIntegerTests
{
    [TestCase("0", 0U)]
    [TestCase("1", 1U)]
    [TestCase("2", 2U)]
    [TestCase("3", 3U)]
    [TestCase("4", 4U)]
    [TestCase("5", 5U)]
    [TestCase("6", 6U)]
    [TestCase("7", 7U)]
    [TestCase("8", 8U)]
    [TestCase("9", 9U)]
    [TestCase("123", 123U)]
    [TestCase("4294967295", uint.MaxValue)]
    [TestCase("4294967296", 429496729U)]
    [TestCase("4294967999", 429496799U)]
    [TestCase("0x0", 0U)]
    [TestCase("0x1234", 4660U)]
    [TestCase("0xFFFFFFFF", uint.MaxValue)]
    [TestCase("0123", 83U)]
    [TestCase("00", 0U)]
    [TestCase("037777777777", uint.MaxValue)]
    public void Parse_Test(string test, uint value)
    {
        var parser = UnsignedInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a")]
    [TestCase("-")]
    [TestCase("X")]
    [TestCase("-0x80000000")]
    [TestCase("-2147483648")]
    [TestCase("-0123")]
    [TestCase("-0")]
    [TestCase("-0x0")]
    [TestCase("-00")]
    [TestCase("-123")]
    public void Parse_Fail(string test)
    {
        var parser = UnsignedInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
