using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers;

public class SynchronizeParserTests
{
    [Test]
    public void Parse_Success()
    {
        var parser = Synchronize(Match('A'), x => true);
        var result = parser.Parse("A;B;C;D");
        result.Success.Should().BeTrue();
        result.Value.Should().Be('A');
    }

    [Test]
    public void Parse_Panic1()
    {
        var parser = Synchronize(Match('B'), x => x == ';');
        var result = parser.Parse("A;B;C;D");
        result.Success.Should().BeFalse();
        var errors = result.TryGetData<ErrorList>().Value;
        errors.ErrorResults.Count.Should().Be(1);
        var finalResult = result.TryGetData<IResult<char>>().Value;
        finalResult.Success.Should().BeTrue();
        finalResult.Value.Should().Be('B');
    }
}
