using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class SetDataGetDataTests
{
    [Test]
    public void Parse_Test()
    {
        var target = Rule(
            SetData("test", "value"),
            GetData<string>("test"),
            (a, b) => $"{a}{b}"
        );
        var result = target.Parse("");
        result.Success.Should().BeTrue();
        result.Value.Should().Be("valuevalue");
    }
}
