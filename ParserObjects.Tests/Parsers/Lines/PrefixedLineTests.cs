using static ParserObjects.Parsers;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers.Lines;

internal class PrefixedLineTests
{
    [Test]
    public void Parse_NoPrefix()
    {
        var parser = PrefixedLine("");
        var input = FromString(@"line
NOT LINE");
        parser.Parse(input).Value.Should().Be("line");
    }

    [Test]
    public void Parse_Prefix()
    {
        var parser = PrefixedLine("XXX");
        var input = FromString(@"XXXline
NOT LINE");
        parser.Parse(input).Value.Should().Be("XXXline");
    }

    [Test]
    public void Parse_Prefix_Fail()
    {
        var parser = PrefixedLine("YYY");
        var input = FromString(@"XXXline
NOT LINE");
        parser.Parse(input).Success.Should().BeFalse();
    }
}
