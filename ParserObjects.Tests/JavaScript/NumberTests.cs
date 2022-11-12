﻿using ParserObjects.Sequences;
using static ParserObjects.JavaScriptStyleParserMethods;

namespace ParserObjects.Tests.JavaScript;

internal class NumberTests
{
    [Test]
    public void JavaScriptStyleNumberLiteral_Tests()
    {
        var parser = Number();
        var result = parser.Parse(new StringCharacterSequence("-1.23e+4"));
        result.Success.Should().BeTrue();
        result.Value.Should().Be(-12300.0);
    }
}
