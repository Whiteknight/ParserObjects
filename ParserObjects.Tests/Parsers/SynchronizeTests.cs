using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class SynchronizeTests
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

    [Test]
    public void Parse_Panic3()
    {
        var parser = Synchronize(Match('D'), x => x == ';');
        var result = parser.Parse("A;B;C;D");
        result.Success.Should().BeFalse();
        var errors = result.TryGetData<ErrorList>().Value;
        errors.ErrorResults.Count.Should().Be(3);
        var finalResult = result.TryGetData<IResult<char>>().Value;
        finalResult.Success.Should().BeTrue();
        finalResult.Value.Should().Be('D');
    }

    [Test]
    public void Parse_PanicEndOfInput()
    {
        var parser = Synchronize(Match('X'), x => x == ';');
        var result = parser.Parse("A;B;C;D");
        result.Success.Should().BeFalse();
        var errors = result.TryGetData<ErrorList>().Value;
        errors.ErrorResults.Count.Should().Be(4);
        var finalResult = result.TryGetData<IResult<char>>();
        finalResult.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_EndOfInput()
    {
        var parser = Synchronize(Match('X'), x => x == ';');
        var result = parser.Parse("");
        result.Success.Should().BeFalse();
        var errors = result.TryGetData<ErrorList>().Value;
        errors.ErrorResults.Count.Should().Be(1);
        var finalResult = result.TryGetData<IResult<char>>();
        finalResult.Success.Should().BeFalse();
    }

    [Test]
    public void GetChildren_Test()
    {
        var inner = Match('X');
        var parser = Synchronize(inner, x => x == ';');
        var result = parser.GetChildren().ToList();
        result.Count.Should().Be(1);
        result[0].Should().BeSameAs(inner);
    }

    [Test]
    public void SetName_Test()
    {
        var parser = Synchronize(Match('X'), x => x == ';');
        var result = parser.SetName("test");
        result.Name.Should().Be("test");
        parser.Name.Should().Be("");
        result.Should().NotBeSameAs(parser);
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Synchronize(Any(), x => x == 'X').Named("SUT");
        var result = target.ToBnf();
        result.Should().Contain("SUT := ");
    }
}
