using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests.C;

internal class IdentifierTests
{
    [Test]
    public void CStyleIdentifier_Tests()
    {
        var parser = Identifier();
        parser.CanMatch("_").Should().BeTrue();
        parser.CanMatch("a").Should().BeTrue();
        parser.CanMatch("a1").Should().BeTrue();
        parser.CanMatch("_1").Should().BeTrue();
        parser.CanMatch("test").Should().BeTrue();
        parser.CanMatch("0").Should().BeFalse();
        parser.CanMatch("-a").Should().BeFalse();
    }
}
