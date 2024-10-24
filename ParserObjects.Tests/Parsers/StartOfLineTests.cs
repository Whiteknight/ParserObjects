using static ParserObjects.Sequences;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class StartOfLineTests
{
    [Test]
    public void Test()
    {
        var input = FromString("a\nb\nc");
        var target = StartOfLine();

        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        input.Consumed.Should().Be(0);

        input.GetNext().Should().Be('a');        // a
        result = target.Parse(input);
        result.Success.Should().BeFalse();

        input.GetNext().Should().Be('\n');        // \n
        result = target.Parse(input);
        result.Success.Should().BeTrue();

        input.GetNext().Should().Be('b');        // b
        result = target.Parse(input);
        result.Success.Should().BeFalse();

        input.GetNext().Should().Be('\n');        // \n
        result = target.Parse(input);
        result.Success.Should().BeTrue();
    }
}
