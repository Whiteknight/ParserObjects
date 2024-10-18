using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors;

public class GetAllParsersTests
{
    [Test]
    public void GetAllParsers_Test()
    {
        var a = MatchChar('a');
        var b = MatchChar('b');
        var c = MatchChar('c');
        var f = First(a, b, c);

        var result = f.GetAllParsers();
        result.Count.Should().Be(4);
        result[a.Id].Should().BeSameAs(a);
        result[b.Id].Should().BeSameAs(b);
        result[c.Id].Should().BeSameAs(c);
        result[f.Id].Should().BeSameAs(f);
    }
}
