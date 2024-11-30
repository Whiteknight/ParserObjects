using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class UnsignedLongIntegerString
{
    [TestCase("0", "0")]
    [TestCase("1", "1")]
    [TestCase("123", "123")]
    [TestCase("0x0", "0x0")]
    [TestCase("0x1234", "0x1234")]
    [TestCase("0123", "0123")]
    public void Parse_Test(string test, string value)
    {
        var parser = UnsignedLongIntegerString();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(value);
    }
}
