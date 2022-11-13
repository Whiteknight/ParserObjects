using static ParserObjects.Parsers.CPP;

namespace ParserObjects.Tests.CPlusPlus;

internal class CommentTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Comment();
        var result = parser.Parse("// TEST\n");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("// TEST");
    }
}
