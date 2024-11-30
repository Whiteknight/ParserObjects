using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class ObjectTests
{
    [Test]
    public void Parse_Test()
    {
        var target = Object(Any());
        var result = target.Parse("abc");
        result.Success.Should().BeTrue();
    }

    [Test]
    public void Match_Test()
    {
        var target = Object(Any());
        var result = target.Match("abc");
        result.Should().BeTrue();
    }

    [Test]
    public void ToBnf()
    {
        var target = Object(Any()).Named("target");
        target.ToBnf().Should().Contain("target := .");
    }

    [Test]
    public void ToString_Test()
    {
        var target = Object(Any()).Named("target");
        target.ToString().Should().NotBeNullOrEmpty();
    }
}
