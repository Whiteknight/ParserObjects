﻿using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class OrTests
{
    private readonly IParser<char, char> _anyParser = Any();
    private readonly IParser<char, char> _failParser = Fail<char>();

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Parse_Test(bool firstSuccess, bool secondSuccess)
    {
        var p1 = firstSuccess ? _anyParser : _failParser;
        var p2 = secondSuccess ? _anyParser : _failParser;
        var target = Or(p1, p2);
        var result = target.Parse("abc");
        result.Success.Should().Be(firstSuccess | secondSuccess);
        result.Consumed.Should().Be(firstSuccess | secondSuccess ? 1 : 0);
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Match_Test(bool firstSuccess, bool secondSuccess)
    {
        var p1 = firstSuccess ? _anyParser : _failParser;
        var p2 = secondSuccess ? _anyParser : _failParser;
        var target = Or(p1, p2);
        var sequence = FromString("abc");
        var result = target.Match(sequence);
        result.Should().Be(firstSuccess | secondSuccess);
        sequence.Consumed.Should().Be(firstSuccess | secondSuccess ? 1 : 0);
    }
}