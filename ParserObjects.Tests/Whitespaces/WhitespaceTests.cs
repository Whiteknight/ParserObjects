using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Whitespaces;

internal class WhitespaceTests
{
    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\r")]
    [TestCase("\n")]
    [TestCase("\v")]
    public void Parse_Test(string test)
    {
        var parser = Whitespace();
        parser.CanMatch(test).Should().BeTrue();
    }

    [TestCase("")]
    [TestCase("x")]
    public void Parse_Fail(string test)
    {
        var parser = Whitespace();
        parser.CanMatch(test).Should().BeFalse();
    }
}
