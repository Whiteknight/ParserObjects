using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class Rule9Tests
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Value.Should().Be("abcdefghi");
        result.Consumed.Should().Be(9);
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
            Match('f'),
            Match('g'),
            Match('h'),
            Match('i')
        ).Rule((a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}");

        var input = FromString("abcdefghijklmn");

        target.Parse(input).Value.Should().Be("abcdefghi");
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(9);
    }

    [Test]
    public void ToBnf()
    {
        var target = (
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            Match('f'),
            Match('g'),
            Match('h'),
            Match('i')
        ).Rule((a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}")
        .Named("parser");
        var result = target.ToBnf();
        result.Should().Contain("parser := ('a' 'b' 'c' 'd' 'e' 'f' 'g' 'h' 'i')");
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
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
            Match('g'),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail7()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            Match('f'),
            Fail<char>(),
            Match('h'),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail8()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            Match('f'),
            Match('g'),
            Fail<char>(),
            Match('i'),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Fail9()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            Match('d'),
            Match('e'),
            Match('f'),
            Match('g'),
            Match('h'),
            Fail<char>(),
            (a, b, c, d, e, f, g, h, i) => $"{a}{b}{c}{d}{e}{f}{g}{h}{i}"
        );

        var input = FromString("abcdefghijklmn");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
