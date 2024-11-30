using static ParserObjects.Parsers.JS;

namespace ParserObjects.Tests.Parsers.JavaScript;

public class StrippedStringTests
{
    [TestCase("'abcd'", "abcd")]
    [TestCase("'\\f\\n\\r\\x0A'", "\f\n\r\x0A")]
    [TestCase("\"abcd\"", "abcd")]
    [TestCase("\"\\f\\n\\r\\x0A\"", "\f\n\r\x0A")]
    [TestCase("'\\u1234'", "\u1234")]
    [TestCase("'\\u{1234}'", "\u1234")]
    [TestCase("'\\n'", "\n")]
    [TestCase("'\\''", "'")]
    [TestCase("\"\\n\"", "\n")]
    [TestCase("\"\\\"\"", "\"")]
    public void Success(string value, string expected)
    {
        var parser = StrippedString();
        var result = parser.Parse(value);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestCase("'\\z'")]
    [TestCase("\"\\z\"")]
    public void Failure(string input)
    {
        var parser = StrippedString();
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
    }
}
