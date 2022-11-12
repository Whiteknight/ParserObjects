using ParserObjects.Sequences;
using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests.C;

internal class DoubleTests
{
    [Test]
    public void CStyleDoubleLiteral_Tests()
    {
        var parser = Double();
        var result = parser.Parse(new StringCharacterSequence("12.34"));
        result.Success.Should().BeTrue();
        result.Value.Should().Be(12.34);
    }
}
