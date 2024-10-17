using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class Rule2Tests
{
    private const string _source = "abcdefghijklmnop";
    private const string _result = "ab";

    private static IParser<char, string> FromMethod() => Rule(
        Match('a'),
        Match('b'),
        (a, b) => $"{a}{b}"
    );

    private static IParser<char, string> FromTuple() => (
        Match('a'),
        Match('b')
    ).Rule((a, b) => $"{a}{b}");

    [Test]
    public void Method()
    {
        var target = FromMethod();
        var input = FromString(_source);
        var result = target.Parse(input);
        result.Value.Should().Be(_result);
        result.Consumed.Should().Be(2);
    }

    [Test]
    public void Tuple()
    {
        var target = FromTuple();
        var input = FromString(_source);
        var result = target.Parse(input);
        result.Value.Should().Be(_result);
        result.Consumed.Should().Be(2);
    }

    [Test]
    public void GetChildren()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            (a, b) => $"{a}{b}"
        );

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(2);
    }

    [Test]
    public void ToBnf()
    {
        var target = Rule(
            Match('a'),
            Match('b'),
            (a, b) => $"{a}{b}"
        )
        .Named("parser");
        var result = target.ToBnf();
        result.Should().Contain("parser := ('a' 'b')");
    }

    [Test]
    public void Fail1()
    {
        var target = Rule(
            Fail<char>(),
            Match('b'),
            (a, b) => $"{a}{b}"
        );
        var input = FromString(_source);
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
            (a, b) => $"{a}{b}"
        );
        var input = FromString(_source);
        var result = target.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }
}
