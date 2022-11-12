using ParserObjects.Sequences;
using static ParserObjects.CPlusPlusStyleParserMethods;

namespace ParserObjects.Tests.CPlusPlus;

internal class CommentTests
{
    [Test]
    public void CPlusPlusStyleCommentLiteral_Tests()
    {
        var parser = Comment();
        var result = parser.Parse(new StringCharacterSequence("// TEST\n"));
        result.Success.Should().BeTrue();
        result.Value.Should().Be("// TEST");
    }
}
