using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.C;

internal class CommentTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Comment();
        var result = parser.Parse("/* TEST */");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("/* TEST */");
    }
}
