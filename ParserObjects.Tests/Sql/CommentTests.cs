using static ParserObjects.Parsers.Sql;

namespace ParserObjects.Tests.Sql;

internal class CommentTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Comment();
        var result = parser.Parse("-- TEST\n");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("-- TEST");
    }
}
