using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class Rule5Tests
{
    [Test]
    public void Method()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Value.Should().Be("abcde");
        result.Consumed.Should().Be(5);
    }

    [Test]
    public void Tuple()
    {
        var target = (
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e')
        ).Rule((a, b, c, d, e) => $"{a}{b}{c}{d}{e}");

        var input = FromString("abcdefghijklmn");

        target.Parse(input).Value.Should().Be("abcde");
    }

    [Test]
    public void GetChildren()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(5);
    }

    [Test]
    public void ToBnf()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        )
        .Named("parser");
        var result = target.ToBnf();
        result.Should().Contain("parser := ('a' 'b' 'c' 'd' 'e')");
    }

    [Test]
    public void Fail1()
    {
        var target = Rule(
            Fail<char>(),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail2()
    {
        var target = Rule(
            Match('a'),
            Fail<char>(),
            Match('c'),
            Match('d'),
            Match('e'),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail3()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Fail<char>(),
            Match('d'),
            Match('e'),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail4()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Fail<char>(),
            Match('e'),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail5()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Fail<char>(),
            (a, b, c, d, e) => $"{a}{b}{c}{d}{e}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
