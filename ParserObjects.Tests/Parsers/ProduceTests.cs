﻿using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class ProduceTests
{
    [Test]
    public void Parse_Test()
    {
        var target = Produce(() => 5);
        var input = FromString("abc");
        var result = target.Parse(input);
        result.Value.Should().Be(5);
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Match_Test()
    {
        var target = Produce(() => 5);
        var input = FromString("abc");
        var result = target.Match(input);
        result.Should().BeTrue();
    }

    [Test]
    public void GetChildren_Test()
    {
        var target = Produce(() => 5);
        target.GetChildren().Count().Should().Be(0);
    }

    [Test]
    public void ToBnf_Test()
    {
        var parser = Produce(() => 'a').Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := PRODUCE");
    }
}
