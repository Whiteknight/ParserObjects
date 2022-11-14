﻿using static ParserObjects.JavaScriptStyleParserMethods;

namespace ParserObjects.Tests.JavaScript;

internal class StringTests
{
    [Test]
    public void String_SingleQuote_Tests()
    {
        var parser = String();
        var result = parser.Parse("'abcd'");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("'abcd'");
    }

    [Test]
    public void String_SingleQuote_Escapes()
    {
        var parser = String();
        var result = parser.Parse("'\\f\\n\\r\\x0A'");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("'\\f\\n\\r\\x0A'");
    }

    [Test]
    public void String_SingleQuote_InvalidEscapes()
    {
        var parser = String();
        var result = parser.Parse("'\\z'");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void String_DoubleQuote_Tests()
    {
        var parser = String();
        var result = parser.Parse("\"abcd\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("\"abcd\"");
    }

    [Test]
    public void String_DoubleQuote_Escapes()
    {
        var parser = String();
        var result = parser.Parse("\"\\f\\n\\r\\x0A\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("\"\\f\\n\\r\\x0A\"");
    }

    [Test]
    public void String_DoubleQuote_InvalidEscapes()
    {
        var parser = String();
        var result = parser.Parse("\"\\z\"");
        result.Success.Should().BeFalse();
    }
}