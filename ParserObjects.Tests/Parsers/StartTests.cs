using static ParserObjects.Sequences;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class StartTests
{
    [Test]
    public void Test()
    {
        var input = FromString("abc");
        var target = Start();
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
    }

    [Test]
    public void Fail()
    {
        var input = FromString("abc");
        var target = Start();
        input.GetNext();
        var result = target.Parse(input);
        result.Success.Should().BeFalse();
    }
}
