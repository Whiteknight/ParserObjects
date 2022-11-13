﻿using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class FollowedByTests
{
    [Test]
    public void Parse_Fail()
    {
        var parser = Match('[').FollowedBy(Match('~'));
        var input = FromString("[test]");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
        input.Peek().Should().Be('[');
    }

    [Test]
    public void Parse_Success()
    {
        var parser = Match('[').FollowedBy(Match('~'));
        var input = FromString("[~test]");
        var result = parser.Parse(input);
        result.Value.Should().Be('[');
        result.Consumed.Should().Be(1);
        input.Peek().Should().Be('~');
    }
}
