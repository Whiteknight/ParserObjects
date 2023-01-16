using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class FailTests
{
    private IParser<char, char> SingleInstance() => Fail();

    private IMultiParser<char, char> MultiInstance() => FailMulti<char>();

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a")]
    [TestCase("1")]
    [TestCase(".")]
    public void Parse_Test(string test)
    {
        var parser = SingleInstance();
        parser.Match(test).Should().BeFalse();
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a")]
    [TestCase("1")]
    [TestCase(".")]
    public void Parse_Multi(string test)
    {
        var parser = MultiInstance();
        parser.Match(test).Should().BeFalse();
    }

    [Test]
    public void GetChildren_Test()
    {
        var parser = Fail<object>();
        parser.GetChildren().Count().Should().Be(0);
    }

    [Test]
    public void ToBnf_Test()
    {
        var parser = Fail<object>().Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := FAIL");
    }
}
