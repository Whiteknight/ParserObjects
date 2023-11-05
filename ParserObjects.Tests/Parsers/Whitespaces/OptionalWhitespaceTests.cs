using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers.Whitespaces;

internal class OptionalWhitespaceTests
{
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\r")]
    [TestCase("\n")]
    [TestCase("\v")]
    public void OptionalWhitespace_Tests(string test)
    {
        var parser = OptionalWhitespace();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Consumed.Should().Be(test.Length);
    }

    [Test]
    public void Parse_EmptyPrefix()
    {
        var parser = OptionalWhitespace();
        // It will find 0 whitespace chars at the beginning of the sequence and return that
        // The "x" will be left on the input sequence.
        var result = parser.Parse("x");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }
}
