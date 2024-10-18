using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

internal class CharacterTests
{
    [Test]
    [TestCase("'a'", "'a'")]
    [TestCase("'\\n'", "'\\n'")]
    [TestCase("'\\101'", "'\\101'")]
    [TestCase("'\\x60'", "'\\x60'")]
    [TestCase("'\\x060'", "'\\x060'")]
    [TestCase("'\\x0060'", "'\\x0060'")]
    [TestCase("'\\u0060'", "'\\u0060'")]
    [TestCase("'\\U00000060'", "'\\U00000060'")]
    public void Parse_Test(string input, string expected)
    {
        var parser = Character();
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestCase("'\\b'", '\b')]
    [TestCase("'\\f'", '\f')]
    [TestCase("'\\n'", '\n')]
    [TestCase("'\\r'", '\r')]
    [TestCase("'\\t'", '\t')]
    [TestCase("'\\v'", '\v')]
    [TestCase("'\\\\'", '\\')]
    [TestCase("'\\?'", '?')]
    [TestCase("'\\''", '\'')]
    [TestCase("'\\0'", '\0')]
    public void Parse_Escapes(string test, char value)
    {
        var parser = Character();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(test);
    }

    public void Parse_Escapes_a()
    {
        // For whatever reason, the test runner doesn't like it when this one is in TestCase()
        var test = "'\\a'";
        var parser = Character();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(test);
    }

    [Test]
    public void Parse_InvalidEscapes()
    {
        var parser = Character();
        var result = parser.Parse("'\\z'");
        result.Success.Should().BeFalse();
    }

    [Test]
    [TestCase("")]
    [TestCase("'")]
    [TestCase("test'")]
    [TestCase("'\\'")]
    [TestCase("'\\x'")]
    [TestCase("'\\u'")]
    [TestCase("'\\U'")]
    public void Parse_Fail(string input)
    {
        var parser = Character();
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
    }
}
