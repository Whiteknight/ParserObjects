using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.C;

internal class IdentifierTests
{
    [TestCase("_", true)]
    [TestCase("a", true)]
    [TestCase("_a", true)]
    [TestCase("_a_", true)]
    [TestCase("a1", true)]
    [TestCase("_1", true)]
    [TestCase("_1_", true)]
    [TestCase("test", true)]
    [TestCase("0", false)]
    [TestCase("-a", false)]
    public void Parse_Test(string test, bool shouldMatch)
    {
        var parser = Identifier();
        parser.Match(test).Should().Be(shouldMatch);
    }
}
