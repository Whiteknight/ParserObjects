using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers;

internal class ThenTests
{
    private readonly IParser<char, char> _successParser = Any();
    private readonly IParser<char, bool> _failParser = Fail<bool>();

    [Test]
    public void ExtThen_Success_ThenSuccess()
    {
        var parser = _successParser.Then(Any());

        var input = new StringCharacterSequence("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be('b');
        result.Consumed.Should().Be(2);
    }

    [Test]
    public void ExtThen_Fail_ThenSuccess()
    {
        var parser = _failParser.Then(Any());

        var input = new StringCharacterSequence("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
