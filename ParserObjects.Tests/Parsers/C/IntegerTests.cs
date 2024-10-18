using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class IntegerTests
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
    [TestCase("2147483647", int.MaxValue)]
    [TestCase("2147483648", 214748364)]
    [TestCase("2147489999", 214748999)]
    [TestCase("-2147483648", int.MinValue)]
    [TestCase("-2147483649", -214748364)]
    [TestCase("-2147489999", -214748999)]
    [TestCase("0x0", 0)]
    [TestCase("-0x0", 0)]
    [TestCase("0x1234", 4660)]
    [TestCase("0x7FFFFFFF", int.MaxValue)]
    [TestCase("-0x80000000", int.MinValue)]
    [TestCase("0x80000000", int.MinValue)]
    [TestCase("0xFFFFFFFF", -1)]
    [TestCase("0123", 83)]
    [TestCase("-0123", -83)]
    [TestCase("00", 0)]
    [TestCase("-00", 0)]
    [TestCase("017777777777", int.MaxValue)]
    public void Parse_Test(string test, int value)
    {
        var parser = Integer();
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
        var parser = Integer();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
