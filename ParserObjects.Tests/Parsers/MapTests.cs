using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class MapTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Any().Map(c => int.Parse(c.ToString()));
        parser.Parse("1").Value.Should().Be(1);
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Any().Map(c => c).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := .");
    }
}
