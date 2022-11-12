using ParserObjects.Sequences;
using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Lines;

internal class PrefixedLineTests
{
    [Test]
    public void PrefixedLine_NoPrefix()
    {
        var parser = PrefixedLine("");
        var input = new StringCharacterSequence(@"line
NOT LINE");
        parser.Parse(input).Value.Should().Be("line");
    }

    [Test]
    public void PrefixedLine_Prefix()
    {
        var parser = PrefixedLine("XXX");
        var input = new StringCharacterSequence(@"XXXline
NOT LINE");
        parser.Parse(input).Value.Should().Be("XXXline");
    }

    [Test]
    public void PrefixedLine_Prefix_NoMatch()
    {
        var parser = PrefixedLine("YYY");
        var input = new StringCharacterSequence(@"XXXline
NOT LINE");
        parser.Parse(input).Success.Should().BeFalse();
    }
}
