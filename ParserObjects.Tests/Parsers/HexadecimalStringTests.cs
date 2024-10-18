using static ParserObjects.Parsers.Digits;

namespace ParserObjects.Tests.Parsers;

public class HexadecimalStringTests
{
    [Test]
    public void Parse_Test()
    {
        var target = HexadecimalString();
        var result = target.Parse("1aB3");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("1aB3");
    }
}
