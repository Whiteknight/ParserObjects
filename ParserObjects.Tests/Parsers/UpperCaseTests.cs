using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class UpperCaseTests
{
    [TestCase("A", true)]
    [TestCase("a", false)]
    [TestCase("1", false)]
    [TestCase(" ", false)]
    [TestCase("$", false)]
    public void Test(string input, bool shouldMatch)
    {
        var target = UpperCase();
        var result = target.Parse(input);
        result.Success.Should().Be(shouldMatch);
        if (shouldMatch)
            result.Value.Should().Be(input[0]);
    }
}
