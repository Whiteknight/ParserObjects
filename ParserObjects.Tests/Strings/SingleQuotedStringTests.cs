using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Strings;

internal class SingleQuotedStringTests
{
    [Test]
    public void SingleQuotedStringWithEscapedQuotes_Tests()
    {
        var parser = SingleQuotedString();
        var result = parser.Parse("'TEST'");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("'TEST'");
    }
}
