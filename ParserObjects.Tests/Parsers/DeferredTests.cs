﻿using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class DeferredTests
{
    [Test]
    public void Parse_Any()
    {
        var target = Deferred(() => Any());
        var input = FromString("abc");
        target.Parse(input).Value.Should().Be('a');
        target.Parse(input).Value.Should().Be('b');
        target.Parse(input).Value.Should().Be('c');
        target.Parse(input).Success.Should().BeFalse();
    }

    [Test]
    public void Match_Any()
    {
        var target = Deferred(() => Any());
        var input = FromString("abc");
        target.Match(input).Should().BeTrue();
        input.Consumed.Should().Be(1);
    }

    [Test]
    public void GetChildren_Test()
    {
        var anyParser = Any();
        var target = Deferred(() => anyParser);
        var result = target.GetChildren().ToList();
        result.Count().Should().Be(1);
        result[0].Should().BeSameAs(anyParser);
    }

    [Test]
    public void Parse_null()
    {
        var target = Deferred(() => (IParser<char, char>)null);
        Action act = () => target.Parse("abc");
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Parse_Multi()
    {
        var target = Deferred(() => ProduceMulti(() => new[] { "a", "b", "c" }));
        var result = target.Parse(FromString(""));
        result.Success.Should().BeTrue();
        result.Results.Count.Should().Be(3);
        result.Results.Should().Contain(r => r.Value == "a");
        result.Results.Should().Contain(r => r.Value == "b");
        result.Results.Should().Contain(r => r.Value == "c");
    }

    [Test]
    public void ToBnf_Single()
    {
        var parser = Deferred(() => Any()).Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := .");
    }

    [Test]
    public void ToBnf_Multi()
    {
        var target = Deferred(() => ProduceMulti(() => new[] { "a", "b", "c" }));
        var result = target.ToBnf();
        result.Should().Contain("(TARGET) := PRODUCE");
    }
}
