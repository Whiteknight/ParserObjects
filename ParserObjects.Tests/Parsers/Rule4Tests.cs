using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class Rule4Tests
{
    [Test]
    public void Method()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            (a, b, c, d) => $"{a}{b}{c}{d}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Value.Should().Be("abcd");
        result.Consumed.Should().Be(4);
    }

    [Test]
    public void Tuple()
    {
        var target = (
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d')
        ).Rule((a, b, c, d) => $"{a}{b}{c}{d}");

        var input = FromString("abcdefghijklmn");

        target.Parse(input).Value.Should().Be("abcd");
    }

    [Test]
    public void GetChildren()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            (a, b, c, d) => $"{a}{b}{c}{d}"
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(4);
    }

    [Test]
    public void ToBnf()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            (a, b, c, d) => $"{a}{b}{c}{d}"
        )
        .Named("parser");
        var result = target.ToBnf();
        result.Should().Contain("parser := ('a' 'b' 'c' 'd')");
    }

    [Test]
    public void Fail1()
    {
        var target = Rule(
            Fail<char>(),
            Match('b'),
            Match('c'),
            Match('d'),
            (a, b, c, d) => $"{a}{b}{c}{d}"
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
            (a, b, c, d) => $"{a}{b}{c}{d}"
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
            (a, b, c, d) => $"{a}{b}{c}{d}"
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
            (a, b, c, d) => $"{a}{b}{c}{d}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
