using static ParserObjects.Parsers.C;

namespace ParserObjects.Tests.Parsers.C;

internal class CommentTests
{
    [TestCase("/* TEST */")]
    [TestCase("/* \nTEST \n*/")]
    [TestCase("/*****/")]
    public void Parse_Test(string input)
    {
        var parser = Comment();
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be(input);
    }

    [TestCase("/* TEST")]
    [TestCase("/* TEST *")]
    [TestCase("/")]
    [TestCase("")]
    [TestCase("TEST */")]
    public void Parse_Fail(string input)
    {
        var parser = Comment();
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
    }
}
