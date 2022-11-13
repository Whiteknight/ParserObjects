using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.C;

internal class StrippedStringTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = StrippedString();
        var result = parser.Parse("\"abcd\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abcd");
    }

    [Test]
    public void Parse_Empty()
    {
        var parser = StrippedString();
        var result = parser.Parse("\"\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }

    [Test]
    [TestCase("\"\\\"\"", "\"")]
    [TestCase("\"\\a\"", "\a")]
    [TestCase("\"\\b\"", "\b")]
    [TestCase("\"\\n\"", "\n")]
    [TestCase("\"\\r\"", "\r")]
    [TestCase("\"\\v\"", "\v")]
    [TestCase("\"\\101\"", "A")]
    [TestCase("\"\\46\"", "&")]
    [TestCase("\"\\x61\"", "a")]
    [TestCase("\"\\x061\"", "a")]
    [TestCase("\"\\x0061\"", "a")]
    [TestCase("\"\\u0061\"", "a")]
    [TestCase("\"\\U00000061\"", "a")]
    public void Parse_Escapes(string input, string expected)
    {
        var parser = StrippedString();
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Test]
    public void Parse_InvalidEscapes()
    {
        var parser = StrippedString();
        var result = parser.Parse("\"\\z\"");
        result.Success.Should().BeFalse();
    }

    [Test]
    [TestCase("")]
    [TestCase("\"")]
    [TestCase("test\"")]
    [TestCase("\"\\\"")]
    [TestCase("\"\\x\"")]
    [TestCase("\"\\u\"")]
    [TestCase("\"\\U\"")]
    public void Parse_Fail(string attempt)
    {
        var parser = StrippedString();
        var result = parser.Parse(attempt);
        result.Success.Should().BeFalse();
    }
}
