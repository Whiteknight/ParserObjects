using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class LongIntegerTests
{
    [TestCase("0", 0)]
    [TestCase("-0", 0)]
    [TestCase("1", 1)]
    [TestCase("2", 2)]
    [TestCase("3", 3)]
    [TestCase("4", 4)]
    [TestCase("5", 5)]
    [TestCase("6", 6)]
    [TestCase("7", 7)]
    [TestCase("8", 8)]
    [TestCase("9", 9)]
    [TestCase("123", 123)]
    [TestCase("-123", -123)]
    [TestCase("9223372036854775807", long.MaxValue)]
    [TestCase("9223372036854775808", 922337203685477580L)]
    [TestCase("9223372036854799999", 922337203685479999L)]
    [TestCase("-9223372036854775808", long.MinValue)]
    [TestCase("-9223372036854775809", -922337203685477580L)]
    [TestCase("-9223372036854779999", -922337203685477999L)]
    [TestCase("0x0", 0)]
    [TestCase("-0x0", 0)]
    [TestCase("0x1234", 4660)]
    [TestCase("0x7FFFFFFFFFFFFFFF", long.MaxValue)]
    [TestCase("-0x8000000000000000", long.MinValue)]
    [TestCase("0x8000000000000000", long.MinValue)]
    [TestCase("0xFFFFFFFFFFFFFFFF", -1)]
    [TestCase("0123", 83)]
    [TestCase("-0123", -83)]
    [TestCase("00", 0)]
    [TestCase("-00", 0)]
    [TestCase("0777777777777777777777", long.MaxValue)]
    public void Parse_Test(string test, long value)
    {
        var parser = LongInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a")]
    [TestCase("-")]
    [TestCase("X")]
    public void Parse_Fail(string test)
    {
        var parser = LongInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
