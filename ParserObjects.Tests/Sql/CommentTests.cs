using ParserObjects.Sequences;
using static ParserObjects.SqlStyleParserMethods;

namespace ParserObjects.Tests.Sql;

internal class CommentTests
{
    [Test]
    public void SqlStyleCommentLiteral_Tests()
    {
        var parser = Comment();
        var result = parser.Parse(new StringCharacterSequence("-- TEST\n"));
        result.Success.Should().BeTrue();
        result.Value.Should().Be("-- TEST");
    }
}
