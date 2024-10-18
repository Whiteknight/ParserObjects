using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors;

public class FindNamedTests
{
    [Test]
    public void FindNamed_Test()
    {
        var needle = Fail<char>().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = haystack.FindNamed("needle");
        result.Success.Should().BeTrue();
        result.Value.Should().BeSameAs(needle);
    }

    [Test]
    public void FindNamed_Fail()
    {
        var needle = Fail<char>().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = haystack.FindNamed("missing");
        result.Success.Should().BeFalse();
    }
}
