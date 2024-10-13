using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public static class DigitStringTests
{
    public class NoBounds
    {
        [TestCase("1")]
        [TestCase("12")]
        [TestCase("123")]
        [TestCase("1234")]
        [TestCase("12345")]
        public void Test(string value)
        {
            var target = DigitString();
            var result = target.Parse(value);
            result.Success.Should().Be(true);
            result.Value.Should().Be(value);
        }
    }

    public class Bounds
    {
        [TestCase("1", "", false)]
        [TestCase("12", "12", true)]
        [TestCase("123", "123", true)]
        [TestCase("1234", "1234", true)]
        [TestCase("12345", "1234", true)]
        public void Test(string input, string expected, bool shouldSucceed)
        {
            var target = DigitString(2, 4);
            var result = target.Parse(input);
            result.Success.Should().Be(shouldSucceed);
            if (shouldSucceed)
                result.Value.Should().Be(expected);
        }
    }
}
