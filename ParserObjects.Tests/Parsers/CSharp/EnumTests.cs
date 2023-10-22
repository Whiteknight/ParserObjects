using static ParserObjects.Parsers.CSharp;

namespace ParserObjects.Tests.Parsers.CSharp;

public class EnumTests
{
    [TestCase("First", TestEnum.First)]
    [TestCase("Second", TestEnum.Second)]
    [TestCase("Third", TestEnum.Third)]
    public void Test(string input, TestEnum expected)
    {
        var target = Enum<TestEnum>();
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    public enum TestEnum
    {
        First,
        Second = 5,
        Third = 10
    }
}
