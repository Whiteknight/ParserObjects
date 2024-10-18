using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class MatchItemTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = MatchItem(char.IsNumber);
        var result = parser.Parse("123");
        result.Value.Should().Be('1');
        result.Consumed.Should().Be(1);
    }

    [Test]
    public void Parse_Fail()
    {
        var parser = MatchItem(char.IsLetter);
        var result = parser.Parse("123");
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void GetChildren_Test()
    {
        var parser = MatchItem(char.IsNumber);
        parser.GetChildren().Count().Should().Be(0);
    }

    [Test]
    public void MatchEndFails()
    {
        // We cannot match the end sentinel, even explicitly
        var parser = MatchItem(c => c == '\0');
        var result = parser.Parse("");
        result.Success.Should().BeFalse();
    }

    [Test]
    public void DoesNotIncludeEndSentinel()
    {
        var parser = MatchItem(c => char.IsLetterOrDigit(c) || c == '.').ListCharToString();
        var result = parser.Parse("abc");
        result.Value.Should().Be("abc");
    }

    [Test]
    public void DoesNotIncludeOneEndSentinelInLoop()
    {
        // MatchItem(c => ...) will not match an end sentinel, so we should not see one here
        // In comparison to Match(), which does.
        var parser = MatchItem(c => c != ']').ListCharToString();
        var result = parser.Parse("abc");
        result.Value.Should().Be("abc");
    }
}
