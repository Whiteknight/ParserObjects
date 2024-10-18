using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors;

public class FindTests
{
    [Test]
    public void Find_Test()
    {
        var needle = Any();
        var haystack = First(Fail<char>(), needle);
        var result = haystack.Find(needle.Id);
        result.Success.Should().BeTrue();
        result.Value.Should().BeSameAs(needle);
    }

    [Test]
    public void Find_Fail()
    {
        var needle = Any();
        var haystack = First(Fail<char>(), needle);
        var result = haystack.Find(-1);
        result.Success.Should().BeFalse();
    }
}
