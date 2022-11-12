using ParserObjects.Sequences;
using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests.Lines;

internal class LineTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Line();
        var input = new StringCharacterSequence(@"line
NOT LINE");
        var result = parser.Parse(input);
        result.Value.Should().Be("line");
    }
}
