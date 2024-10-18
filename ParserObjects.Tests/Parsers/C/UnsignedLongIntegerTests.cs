using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class UnsignedLongIntegerTests
{
    [TestCase("0", 0UL)]
    [TestCase("1", 1UL)]
    [TestCase("2", 2UL)]
    [TestCase("3", 3UL)]
    [TestCase("4", 4UL)]
    [TestCase("5", 5UL)]
    [TestCase("6", 6UL)]
    [TestCase("7", 7UL)]
    [TestCase("8", 8UL)]
    [TestCase("9", 9UL)]
    [TestCase("123", 123UL)]
    [TestCase("18446744073709551615", ulong.MaxValue)]
    [TestCase("18446744073709551616", 1844674407370955161UL)]
    [TestCase("18446744073709559999", 1844674407370955999UL)]
    [TestCase("0x0", 0UL)]
    [TestCase("0x1234", 4660UL)]
    [TestCase("0xFFFFFFFFFFFFFFFF", ulong.MaxValue)]
    [TestCase("0123", 83UL)]
    [TestCase("00", 0UL)]
    [TestCase("01777777777777777777777", ulong.MaxValue)]
    public void Parse_Test(string test, ulong value)
    {
        var parser = UnsignedLongInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a")]
    [TestCase("-")]
    [TestCase("X")]
    [TestCase("-0")]
    [TestCase("-123")]
    [TestCase("-0x0")]
    [TestCase("-0x8000000000000000")]
    [TestCase("-0123")]
    [TestCase("-00")]
    public void Parse_Fail(string test)
    {
        var parser = UnsignedLongInteger();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
