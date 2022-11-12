using ParserObjects.Sequences;
using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests.C;

internal class CommentTests
{
    [Test]
    public void CStyleCommentLiteral_Tests()
    {
        var parser = Comment();
        var result = parser.Parse(new StringCharacterSequence("/* TEST */"));
        result.Success.Should().BeTrue();
        result.Value.Should().Be("/* TEST */");
    }
}
