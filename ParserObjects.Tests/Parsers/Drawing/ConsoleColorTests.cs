using static ParserObjects.Parsers.Drawing;

namespace ParserObjects.Tests.Parsers.Drawing;

public class ConsoleColorTests
{
    [TestCase("black", System.ConsoleColor.Black)]
    [TestCase("Black", System.ConsoleColor.Black)]
    public void Test(string input, System.ConsoleColor expected)
    {
        var parser = ConsoleColor();
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }
}
