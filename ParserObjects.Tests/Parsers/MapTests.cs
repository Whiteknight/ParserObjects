using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers;

public class MapTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Any().Map(c => int.Parse(c.ToString()));
        parser.Parse("1").Value.Should().Be(1);
    }
}
