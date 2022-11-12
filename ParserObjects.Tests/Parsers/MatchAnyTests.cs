﻿using System.Linq;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Parsers;

public class MatchAnyTests
{
    [Test]
    public void Parse_Operators()
    {
        var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });

        var input = new StringCharacterSequence("===>=<=><<==");

        target.Parse(input).Value.Should().Be("==");
        target.Parse(input).Value.Should().Be("=");
        target.Parse(input).Value.Should().Be(">=");
        target.Parse(input).Value.Should().Be("<=");
        target.Parse(input).Value.Should().Be(">");
        target.Parse(input).Value.Should().Be("<");
        target.Parse(input).Value.Should().Be("<=");
        target.Parse(input).Value.Should().Be("=");
    }

    [Test]
    public void Parse_IsAtEnd()
    {
        var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });

        var input = new StringCharacterSequence("");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void GetChildren_Test()
    {
        var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });
        target.GetChildren().Count().Should().Be(0);
    }

    [Test]
    public void Parse_Operators_Fail()
    {
        var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });

        var input = new StringCharacterSequence("X===>=<=><<==");

        target.Parse(input).Success.Should().BeFalse();
    }
}
