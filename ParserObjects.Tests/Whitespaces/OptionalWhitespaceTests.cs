using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Whitespaces;

internal class OptionalWhitespaceTests
{
    [Test]
    public void OptionalWhitespace_Tests()
    {
        var parser = OptionalWhitespace();
        parser.CanMatch("").Should().BeTrue();
        parser.CanMatch(" ").Should().BeTrue();
        parser.CanMatch("\t").Should().BeTrue();
        parser.CanMatch("\r").Should().BeTrue();
        parser.CanMatch("\n").Should().BeTrue();
        parser.CanMatch("\v").Should().BeTrue();

        // It will find 0 whitespace chars at the beginning of the sequence and return that
        // The "x" will be left on the input sequence.
        var result = parser.Parse("x");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }
}
