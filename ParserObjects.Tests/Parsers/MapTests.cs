using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class MapTests
{
    [Test]
    public void Parse_Test()
    {
        var parser = Any().Map(c => int.Parse(c.ToString()));
        parser.Parse("1").Value.Should().Be(1);
    }

    [Test]
    public void Parse_Failure()
    {
        var fail = Fail<char>();
        var parser = fail.Map(
            c => int.Parse(c.ToString())
        );
        var result = parser.Parse("1");
        result.Success.Should().BeFalse();
        result.Parser.Should().BeSameAs(fail);
    }

    [Test]
    public void Parse_Failure_ErrorMapper()
    {
        var fail = Fail<char>();
        var parser = fail.Map(
            c => int.Parse(c.ToString()),
            (e, l) => -100
        );
        var result = parser.Parse("1");
        result.Success.Should().BeTrue();
        result.Value.Should().Be(-100);
        result.Parser.Should().BeSameAs(parser);
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Any().Map(c => c).Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := .");
    }
}
