using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

public class HexadecimalStringTests
{
    [TestCase("0x0")]
    [TestCase("0x1")]
    [TestCase("0x2")]
    [TestCase("0x3")]
    [TestCase("0x4")]
    [TestCase("0x5")]
    [TestCase("0x6")]
    [TestCase("0x7")]
    [TestCase("0x8")]
    [TestCase("0x9")]
    [TestCase("0xA")]
    [TestCase("0xB")]
    [TestCase("0xC")]
    [TestCase("0xD")]
    [TestCase("0xE")]
    [TestCase("0xF")]
    [TestCase("0xa")]
    [TestCase("0xb")]
    [TestCase("0xc")]
    [TestCase("0xd")]
    [TestCase("0xe")]
    [TestCase("0xf")]
    [TestCase("0xAB12")]
    public void Parse_Test(string test)
    {
        var parser = HexadecimalString();
        var result = parser.Parse(test);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(test);
    }

    [TestCase("0xG")]
    [TestCase("0x_")]
    [TestCase("0x-5")]
    public void Parse_Fail(string test)
    {
        var parser = HexadecimalString();
        var result = parser.Parse(test);
        result.Success.Should().BeFalse();
    }
}
