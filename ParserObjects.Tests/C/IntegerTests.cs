using ParserObjects.Sequences;
using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests.C;

internal class IntegerTests
{
    [Test]
    public void CStyleIntegerLiteral_Tests()
    {
        var parser = Integer();
        var result = parser.Parse(new StringCharacterSequence("1234"));
        result.Success.Should().BeTrue();
        result.Value.Should().Be(1234);
    }
}
