﻿using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class SingleTests
{
    [Test]
    public void Parse_Test()
    {
        var target = ProduceMulti(() => "a")
            .Single();
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be('a');
    }

    [Test]
    public void Parse_FailNoResults()
    {
        var target = ProduceMulti(() => "")
            .Single();
        var result = target.Parse("");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_FailTooMany()
    {
        var target = ProduceMulti(() => "abc")
            .Single();
        var result = target.Parse("");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = ProduceMulti(() => "abc").Single().Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := SELECT PRODUCE");
    }
}
