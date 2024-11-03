﻿using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class SelectTests
{
    [Test]
    public void Parse_Test()
    {
        var target = ProduceMulti(() => new[] { 'A', 'B', 'C' })
            .Select(multi => multi.Results[1]);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be('B');
    }

    [Test]
    public void Parse_Fail()
    {
        var target = ProduceMulti(() => new[] { 'A', 'B', 'C' })
            .Select(_ => default);
        var result = target.Parse("");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = ProduceMulti(() => new[] { 'A', 'B', 'C' })
            .Select(multi => multi.Results[1]);

        var result = target.ToBnf();

        result.Should().Contain("(TARGET) := SELECT PRODUCE");
    }
}
