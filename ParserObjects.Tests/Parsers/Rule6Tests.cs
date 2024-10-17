using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class Rule6Tests
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Value.Should().Be("abcdef");
        result.Consumed.Should().Be(6);
    }

    [Test]
    public void Tuple()
    {
        var target = (
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            Match('f')
        ).Rule((a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}");

        var input = FromString("abcdefghijklmn");

        target.Parse(input).Value.Should().Be("abcdef");
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(6);
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
        )
        .Named("parser");
        var result = target.ToBnf();
        result.Should().Contain("parser := ('a' 'b' 'c' 'd' 'e' 'f')");
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
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
            Match('f'),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail6()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            Fail<char>(),
            (a, b, c, d, e, f) => $"{a}{b}{c}{d}{e}{f}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
