using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class ObjectTests
{
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
