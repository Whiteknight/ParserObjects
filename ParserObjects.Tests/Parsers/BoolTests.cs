using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class BoolTests
{
    [Test]
    public void Parse_True()
    {
        var target = Bool(End());
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public void Parse_False()
    {
        var target = Bool(End());
        var result = target.Parse("xxx");
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Bool(Any()).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := IF .");
    }
}
