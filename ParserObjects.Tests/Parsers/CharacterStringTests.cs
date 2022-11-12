using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class CharacterStringTests
{
    [Test]
    public void CharacterString_Test()
    {
        var parser = ParserMethods.CharacterString("abc");
        var input = new StringCharacterSequence("abcd");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abc");
        result.Consumed.Should().Be(3);
    }
}
