using ParserObjects.Internal;

namespace ParserObjects.Tests;

public class PartialResultTests
{
    [Test]
    public void Match_Success()
    {
        var target = new PartialResult<int>(5, 0);
        var result = target.Match(x => x.ToString(), () => "FAIL");
        result.Should().Be("5");
    }

    [Test]
    public void Match_Error()
    {
        var target = new PartialResult<int>("ErrorMessage");
        var result = target.Match(x => x.ToString(), () => "FAIL");
        result.Should().Be("FAIL");
    }
}
