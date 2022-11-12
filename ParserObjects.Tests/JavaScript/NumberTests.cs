using static ParserObjects.JavaScriptStyleParserMethods;

namespace ParserObjects.Tests.JavaScript;

internal class NumberTests
{
    [TestCase("0", 0.0)]
    [TestCase("-0", 0.0)]
    [TestCase("0.0", 0.0)]
    [TestCase("123", 123.0)]
    [TestCase("-123", -123.0)]
    [TestCase("-1.23e+4", -12300.0)]
    public void Parse_Test(string test, double value)
    {
        var parser = Number();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }
}
