using static ParserObjects.Sequences;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

internal class EndOfLineTests
{
    [Test]
    public void Test()
    {
        var input = FromString("a\nb");
        var target = EndOfLine();
        var result = target.Parse(input);
        result.Success.Should().BeFalse();

        input.GetNext().Should().Be('a');
        result = target.Parse(input);
        result.Success.Should().BeTrue();

        input.GetNext().Should().Be('\n');
        result = target.Parse(input);
        result.Success.Should().BeFalse();

        input.GetNext().Should().Be('b');
        result = target.Parse(input);
        result.Success.Should().BeTrue();
    }
}
