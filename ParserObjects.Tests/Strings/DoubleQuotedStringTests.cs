using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Strings;

internal class DoubleQuotedStringTests
{
    [Test]
    public void DoubleQuotedStringWithEscapedQuotes_Tests()
    {
        var parser = DoubleQuotedString();
        var result = parser.Parse("\"TEST\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("\"TEST\"");

        result = parser.Parse("\"TE\\\"ST\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("\"TE\\\"ST\"");
    }
}
