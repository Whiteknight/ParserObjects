using static ParserObjects.Parsers.C;

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

    [TestCase("'\\101'", 'A')]
    [TestCase("'\\60'", '0')]
    [TestCase("'\\141'", 'a')]
    [TestCase("'\\175'", '}')]
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
