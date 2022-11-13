using static ParserObjects.Parsers;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Lines;

internal class LineTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Line();
        var input = FromString(@"line
NOT LINE");
        var result = parser.Parse(input);
        result.Value.Should().Be("line");
    }
}
