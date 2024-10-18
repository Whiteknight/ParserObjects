using static ParserObjects.Parsers.Cpp;

namespace ParserObjects.Tests.Parsers.CPlusPlus;

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
