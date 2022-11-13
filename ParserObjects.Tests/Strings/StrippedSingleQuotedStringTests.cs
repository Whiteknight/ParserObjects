using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Strings;

internal class StrippedSingleQuotedStringTests
{
    [Test]
    public void StrippedSingleQuotedStringWithEscapedQuotes_Tests()
    {
        var parser = StrippedSingleQuotedString();
        var result = parser.Parse("'TEST'");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("TEST");
    }
}
