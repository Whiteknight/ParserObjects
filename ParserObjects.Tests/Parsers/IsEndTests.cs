using static ParserObjects.Sequences;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

internal class IsEndTests
{
    [Test]
    public void Test()
    {
        var input = FromString("ab");
        var target = IsEnd();
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();

        input.GetNext().Should().Be('a');
        result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();

        input.GetNext().Should().Be('b');
        result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }
}
