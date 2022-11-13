using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Strings;

internal class StrippedDoubleQuotedStringTests
{
    [Test]
    public void StrippedDoubleQuotedStringWithEscapedQuotes_Tests()
    {
        var parser = StrippedDoubleQuotedString();
        var result = parser.Parse("\"TEST\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("TEST");

        result = parser.Parse("\"TE\\\"ST\"");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("TE\"ST");
    }
}
