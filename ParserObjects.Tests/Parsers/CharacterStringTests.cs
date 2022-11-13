using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

internal class CharacterStringTests
{
    [TestCase("abc", "abcd", true)]
    [TestCase("abc", "xyz", false)]
    [TestCase("abc", "abZ", false)]
    [TestCase("abc", "a", false)]
    public void Parse_Test(string pattern, string test, bool shouldMatch)
    {
        var parser = CharacterString(pattern);

        var result = parser.Parse(test);
        result.Success.Should().Be(shouldMatch);
        result.Consumed.Should().Be(shouldMatch ? pattern.Length : 0);
        if (shouldMatch)
            result.Value.Should().Be(pattern);
    }
}
