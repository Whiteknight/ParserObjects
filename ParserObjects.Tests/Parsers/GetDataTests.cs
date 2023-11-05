using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

// Most gets for GetData are included with DataContext() tests.
public class GetDataTests
{
    [Test]
    public void Parse_KeyNotFound()
    {
        var target = GetData<string>("test");
        var result = target.Parse("");
        result.Success.Should().BeFalse();
    }
}
