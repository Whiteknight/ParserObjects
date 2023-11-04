using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class LowerCaseTests
{
    [TestCase("A", false)]
    [TestCase("a", true)]
    [TestCase("1", false)]
    [TestCase(" ", false)]
    [TestCase("$", false)]
    public void Test(string input, bool shouldMatch)
    {
        var target = LowerCase();
        var result = target.Parse(input);
        result.Success.Should().Be(shouldMatch);
        if (shouldMatch)
            result.Value.Should().Be(input[0]);
    }
}
