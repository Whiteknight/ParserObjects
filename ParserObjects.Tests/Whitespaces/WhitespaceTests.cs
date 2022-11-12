using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Whitespaces;

internal class WhitespaceTests
{
    [Test]
    public void Whitespace_Tests()
    {
        var parser = Whitespace();
        parser.CanMatch("").Should().BeFalse();
        parser.CanMatch(" ").Should().BeTrue();
        parser.CanMatch("\t").Should().BeTrue();
        parser.CanMatch("\r").Should().BeTrue();
        parser.CanMatch("\n").Should().BeTrue();
        parser.CanMatch("\v").Should().BeTrue();
        parser.CanMatch("x").Should().BeFalse();
    }
}
