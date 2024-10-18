using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

internal class StringTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = String();
        var result = parser.Parse("\"abcd\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("\"abcd\"");
    }

    [Test]
    public void Parse_Empty()
    {
        var parser = String();
        var result = parser.Parse("\"\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("\"\"");
    }

    [Test]
    [TestCase("\"\\\"\"")]
    [TestCase("\"\\a\"")]
    [TestCase("\"\\b\"")]
    [TestCase("\"\\n\"")]
    [TestCase("\"\\r\"")]
    [TestCase("\"\\v\"")]
    [TestCase("\"\\60\"")]
    [TestCase("\"\\101\"")]
    [TestCase("\"\\x41\"")]
    [TestCase("\"\\x041\"")]
    [TestCase("\"\\x0041\"")]
    [TestCase("\"\\u0026\"")]
    [TestCase("\"\\U00000027\"")]
    public void Parse_Escape(string value)
    {
        var parser = String();
        var result = parser.Parse(value);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Test]
    public void Parse_InvalidEscapes()
    {
        var parser = String();
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
        var parser = String();
        var result = parser.Parse(attempt);
        result.Success.Should().BeFalse();
    }
}
