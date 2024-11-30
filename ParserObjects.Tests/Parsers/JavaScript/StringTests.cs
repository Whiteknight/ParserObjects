using static ParserObjects.Parsers.JS;

namespace ParserObjects.Tests.Parsers.JavaScript;

public class StringTests
{
    [TestCase("'abcd'")]
    [TestCase("'\\f\\n\\r\\x0A'")]
    [TestCase("\"abcd\"")]
    [TestCase("\"\\f\\n\\r\\x0A\"")]
    [TestCase("'\\u1234'")]
    [TestCase("'\\u{1234}'")]
    [TestCase("'\\n'")]
    [TestCase("'\\''")]
    [TestCase("\"\\n\"")]
    [TestCase("\"\n\"")]
    [TestCase("\"\\\"\"")]
    public void Success(string value)
    {
        var parser = String();
        var result = parser.Parse(value);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [TestCase("'\\z'")]
    [TestCase("\"\\z\"")]
    public void Failure(string input)
    {
        var parser = String();
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
    }
}
