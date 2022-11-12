using ParserObjects.Sequences;
using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests.C;

internal class HexadecimalIntegerTests
{
    [Test]
    public void CStyleHexadecimalLiteral_Tests()
    {
        var parser = HexadecimalInteger();
        var result = parser.Parse(new StringCharacterSequence("0xAB12"));
        result.Success.Should().BeTrue();
        result.Value.Should().Be(0xAB12);
    }
}
