using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class CaptureStringTests
{
    [Test]
    public void Parse_Test()
    {
        var target = CaptureString(
            MatchChars("abc"),
            Any(),
            Any(),
            Any(),
            MatchChars("ghi")
        );
        var result = target.Parse("abcdefghi");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abcdefghi");
    }

    [Test]
    public void Parse_NonCharSequence()
    {
        var target = CaptureString(
            MatchChar('a'),
            Any(),
            MatchChar('c')
        );
        var input = FromEnumerable(new[] { (byte)'a', (byte)'b', (byte)'c' }).Select(b => (char)b);
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abc");
    }

    [Test]
    public void Parse_Empty()
    {
        var target = CaptureString();
        var result = target.Parse("abcdefghi");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }

    [Test]
    public void Parse_Null()
    {
        var target = CaptureString(null);
        var result = target.Parse("abcdefghi");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("");
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Capture(Any()).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := .");
    }

    [Test]
    public void ToBnf_2_Test()
    {
        var target = Capture(Any(), Any()).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := . && .");
    }
}
