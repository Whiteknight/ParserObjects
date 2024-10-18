using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class SymbolTests
{
    [TestCase("A", false)]
    [TestCase("a", false)]
    [TestCase("1", false)]
    [TestCase(" ", false)]
    [TestCase("$", true)]
    public void Test(string input, bool shouldMatch)
    {
        var target = Symbol();
        var result = target.Parse(input);
        result.Success.Should().Be(shouldMatch);
        if (shouldMatch)
            result.Value.Should().Be(input[0]);
    }
}
