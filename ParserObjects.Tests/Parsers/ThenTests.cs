using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class ThenTests
{
    private readonly IParser<char, char> _successParser = Any();
    private readonly IParser<char, bool> _failParser = Fail<bool>();

    [Test]
    public void Parse_Success_ThenSuccess()
    {
        var parser = _successParser.Then(Any());

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be('b');
        result.Consumed.Should().Be(2);
    }

    [Test]
    public void Parse_Fail_ThenSuccess()
    {
        var parser = _failParser.Then(Any());

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
