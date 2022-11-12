using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers;

internal class NotFollowedByTests
{
    [Test]
    public void Parse_Success()
    {
        var parser = Match('[').NotFollowedBy(Match("~"));
        var input = new StringCharacterSequence("[test]");
        var result = parser.Parse(input);
        result.Value.Should().Be('[');
        result.Consumed.Should().Be(1);
    }

    [Test]
    public void Parse_Fail()
    {
        var parser = Match('[').NotFollowedBy(Match("~"));
        var input = new StringCharacterSequence("[~test]");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
