using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class FirstResultTests
{
    [Test]
    public void Parse_Test()
    {
        var target = FirstResult(ProduceMulti(() => "a"));
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be('a');
    }

    [Test]
    public void Parse_Test_Predicate()
    {
        var target = FirstResult(ProduceMulti(() => "a"), r => r.Success);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be('a');
    }

    [Test]
    public void Parse_FailNoResults()
    {
        var target = FirstResult(ProduceMulti(() => ""));
        var result = target.Parse("");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_FailNoResults_Predicate()
    {
        var target = FirstResult(ProduceMulti(() => ""), r => r.Success);
        var result = target.Parse("");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_FailTooMany()
    {
        var target = FirstResult(ProduceMulti(() => "abc"));
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be('a');
    }

    [Test]
    public void Parse_FailTooMany_Predicate()
    {
        var target = FirstResult(ProduceMulti(() => "abc"), r => r.Success);
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be('a');
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = FirstResult(ProduceMulti(() => "abc")).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := SELECT PRODUCE");
    }
}
