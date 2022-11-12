using ParserObjects.Sequences;
using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Lines;

internal class PrefixedLineTests
{
    [Test]
    public void Parse_NoPrefix()
    {
        var parser = PrefixedLine("");
        var input = new StringCharacterSequence(@"line
NOT LINE");
        parser.Parse(input).Value.Should().Be("line");
    }

    [Test]
    public void Parse_Prefix()
    {
        var parser = PrefixedLine("XXX");
        var input = new StringCharacterSequence(@"XXXline
NOT LINE");
        parser.Parse(input).Value.Should().Be("XXXline");
    }

    [Test]
    public void Parse_Prefix_Fail()
    {
        var parser = PrefixedLine("YYY");
        var input = new StringCharacterSequence(@"XXXline
NOT LINE");
        parser.Parse(input).Success.Should().BeFalse();
    }
}
