using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class IntegerTests
{
    [TestCase("0", 0)]
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
    [TestCase("-0")]
    public void Parse_Fail(string test)
    {
        var parser = Integer();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
