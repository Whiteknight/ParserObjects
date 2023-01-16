using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class LongestResultTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = LongestResult(
            Each(
                CharacterString("a"),
                CharacterString("ab"),
                CharacterString("abc")
            )
        );
        var result = parser.Parse("abc");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abc");
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = LongestResult(
            Each(
                CharacterString("A"),
                CharacterString("B")
            )
        ).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := SELECT EACH('A' | 'B')");
    }
}
