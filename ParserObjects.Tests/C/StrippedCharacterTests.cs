using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests.C;

internal class StrippedCharacterTests
{
    [Test]
    [TestCase("'a'", 'a')]
    [TestCase("'\\n'", '\n')]
    [TestCase("'\\101'", 'A')]
    [TestCase("'\\x61'", 'a')]
    [TestCase("'\\x061'", 'a')]
    [TestCase("'\\x0061'", 'a')]
    [TestCase("'\\u0061'", 'a')]
    [TestCase("'\\U00000061'", 'a')]
    public void Parse_Test(string input, char expected)
    {
        var parser = StrippedCharacter();
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestCase("'\\a'", '\a')]
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
        var parser = StrippedCharacter();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [TestCase("'\\0'", '\x00')]
    [TestCase("'\\1'", '\x01')]
    [TestCase("'\\2'", '\x02')]
    [TestCase("'\\3'", '\x03')]
    [TestCase("'\\4'", '\x04')]
    [TestCase("'\\5'", '\x05')]
    [TestCase("'\\6'", '\x06')]
    [TestCase("'\\7'", '\x07')]
    public void Parse_Octal(string test, char value)
    {
        var parser = StrippedCharacter();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Test]
    public void Parse_InvalidEscapes()
    {
        var parser = StrippedCharacter();
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
        var parser = StrippedCharacter();
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
    }
}
