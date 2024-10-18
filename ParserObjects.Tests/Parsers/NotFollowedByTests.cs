using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class NotFollowedByTests
{
    [Test]
    public void Parse_Success()
    {
        var parser = MatchChar('[').NotFollowedBy(MatchChar('~'));
        var input = FromString("[test]");
        var result = parser.Parse(input);
        result.Value.Should().Be('[');
        result.Consumed.Should().Be(1);
    }

    [Test]
    public void Parse_Fail()
    {
        var parser = MatchChar('[').NotFollowedBy(MatchChar('~'));
        var input = FromString("[~test]");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Any().NotFollowedBy(MatchChar('X')).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := (. (?! 'X' ))");
    }
}
