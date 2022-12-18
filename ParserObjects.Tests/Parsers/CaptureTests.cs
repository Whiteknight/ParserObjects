using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class CaptureTests
{
    [Test]
    public void Parse_Test()
    {
        var target = Capture(
            Match("abc"),
            Any(),
            Any(),
            Any(),
            Match("ghi")
        ).Transform(c => new string(c));
        var result = target.Parse("abcdefghi");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abcdefghi");
    }
}
