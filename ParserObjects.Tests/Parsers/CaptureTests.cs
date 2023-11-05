using static ParserObjects.Parsers<char>;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class CaptureTests
{
    [Test]
    public void Parse_Test()
    {
        var target = Capture(
            MatchChars("abc"),
            Any(),
            Any(),
            Any(),
            MatchChars("ghi")
        ).Transform(c => new string(c));
        var result = target.Parse("abcdefghi");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abcdefghi");
    }

    [Test]
    public void Parse_Empty()
    {
        var target = Capture().Transform(c => new string(c));
        var result = target.Parse("abcdefghi");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Capture(Any()).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := (.)");
    }
}
