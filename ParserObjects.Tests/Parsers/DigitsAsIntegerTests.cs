using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class DigitsAsIntegerTests
{
    [TestCase("1", 0, false)]
    [TestCase("12", 12, true)]
    [TestCase("123", 123, true)]
    [TestCase("1234", 1234, true)]
    [TestCase("12345", 1234, true)]
    public void Test(string input, int expected, bool shouldSucceed)
    {
        var target = DigitsAsInteger(2, 4);
        var result = target.Parse(input);
        result.Success.Should().Be(shouldSucceed);
        if (shouldSucceed)
            result.Value.Should().Be(expected);
    }
}
