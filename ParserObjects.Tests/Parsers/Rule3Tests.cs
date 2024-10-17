using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

internal class Rule3Tests
{
    [Test]
    public void Method()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            (a, b, c) => $"{a}{b}{c}"
        );

        var input = FromString("abc");

        var result = target.Parse(input);
        result.Value.Should().Be("abc");
        result.Consumed.Should().Be(3);
    }

    [Test]
    public void Tuple()
    {
        var target = (
            Match('a'),
            Match('b'),
            Match('c')
        ).Rule((a, b, c) => $"{a}{b}{c}");

        var input = FromString("abc");

        target.Parse(input).Value.Should().Be("abc");
    }

    [Test]
    public void GetChildren()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            (a, b, c) => $"{a}{b}{c}"
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(3);
    }

    [Test]
    public void ToBnf()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            (a, b, c) => $"{a}{b}{c}"
        )
        .Named("parser");
        var result = target.ToBnf();
        result.Should().Contain("parser := ('a' 'b' 'c')");
    }

    [Test]
    public void RewindInput()
    {
        var parser = Rule(
            Match('a'),
            Match('b'),
            Match('c'),
            (a, b, c) => "ok"
        );
        var input = FromString("abd");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        input.Peek().Should().Be('a');
    }

    [Test]
    public void Fail1()
    {
        var target = Rule(
            Fail<char>(),
            Match('b'),
            Match('c'),
            (a, b, c) => $"{a}{b}{c}"
        );

        var input = FromString("abc");

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
            (a, b, c) => $"{a}{b}{c}"
        );

        var input = FromString("abc");

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
            (a, b, c) => $"{a}{b}{c}"
        );

        var input = FromString("abc");

        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
