using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class FollowedByTests
{
    [Test]
    public void Parse_Fail()
    {
        var parser = Match('[').FollowedBy(Match('~'));
        var input = FromString("[test]");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
        input.Peek().Should().Be('[');
    }

    [Test]
    public void Parse_Untyped()
    {
        var parser = Any().FollowedBy(End());
        parser.Match("a").Should().BeTrue();
        parser.Match("ab").Should().BeFalse();
    }

    [Test]
    public void Parse_Success()
    {
        var parser = Match('[').FollowedBy(Match('~'));
        var input = FromString("[~test]");
        var result = parser.Parse(input);
        result.Value.Should().Be('[');
        result.Consumed.Should().Be(1);
        input.Peek().Should().Be('~');
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Any().FollowedBy(Match('X')).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := (. (?= 'X' ))");
    }
}
