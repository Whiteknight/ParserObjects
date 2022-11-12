using System.Linq;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers;

internal class ChainWithTests
{
    [Test]
    public void Output_Configuration_Test()
    {
        var target = ChainWith<char, char>(Any(), x => x
            .When(c => c == 'a', Match('X'))
            .When(c => c == 'b', Match('Y'))
            .When(c => c == 'c', Match('Z'))
        );
        target.Parse("aX").Success.Should().BeTrue();
        target.Parse("bY").Success.Should().BeTrue();
        target.Parse("cZ").Success.Should().BeTrue();
        target.Parse("aZ").Success.Should().BeFalse();
        target.Parse("dW").Success.Should().BeFalse();
    }

    [Test]
    public void Output_ChainWith_GetChildren_Test()
    {
        var first = Any();
        var x = Match('X');
        var y = Match('Y');
        var z = Match('Z');
        var target = ChainWith<char, char>(first, config => config
            .When(c => c == 'a', x)
            .When(c => c == 'b', y)
            .When(c => c == 'c', z)
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(4);
        result.Should().Contain(first);
        result.Should().Contain(x);
        result.Should().Contain(y);
        result.Should().Contain(z);
    }

    [Test]
    public void NoOutput_Configuration_Test()
    {
        var target = ChainWith<char>((IParser<char>)Any(), x => x
            .When(c => (char)c == 'a', Match('X'))
            .When(c => (char)c == 'b', Match('Y'))
            .When(c => (char)c == 'c', Match('Z'))
        );
        target.Parse("aX").Success.Should().BeTrue();
        target.Parse("bY").Success.Should().BeTrue();
        target.Parse("cZ").Success.Should().BeTrue();
        target.Parse("aZ").Success.Should().BeFalse();
        target.Parse("dW").Success.Should().BeFalse();
    }

    [Test]
    public void NoOutput_ChainWith_GetChildren_Test()
    {
        var first = (IParser<char>)Any();
        var x = Match('X');
        var y = Match('Y');
        var z = Match('Z');
        var target = ChainWith<char>(first, config => config
            .When(c => (char)c == 'a', x)
            .When(c => (char)c == 'b', y)
            .When(c => (char)c == 'c', z)
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(4);
        result.Should().Contain(first);
        result.Should().Contain(x);
        result.Should().Contain(y);
        result.Should().Contain(z);
    }
}
