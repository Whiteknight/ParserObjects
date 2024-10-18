using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class IntegerStringTests
{
    [TestCase("0")]
    [TestCase("-0")]
    [TestCase("1")]
    [TestCase("2")]
    [TestCase("3")]
    [TestCase("4")]
    [TestCase("5")]
    [TestCase("6")]
    [TestCase("7")]
    [TestCase("8")]
    [TestCase("9")]
    [TestCase("123")]
    [TestCase("-123")]
    public void Parse_Test(string test)
    {
        var parser = IntegerString();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(test);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a")]
    [TestCase("-")]
    [TestCase("X")]
    public void Parse_Fail(string test)
    {
        var parser = IntegerString();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
