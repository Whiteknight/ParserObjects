using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public class IfParserTests
{
    private readonly IParser<char, char> _successParser = Any();
    private readonly IParser<char, bool> _failParser = Fail<bool>();

    [Test]
    public void ExtIf_Success_ThenSuccess()
    {
        var parser = Any().If(_successParser);

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be('b');
        result.Consumed.Should().Be(2);
    }

    [Test]
    public void ExtIf_Fail_ThenSuccess()
    {
        var parser = Any().If(_failParser);

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Parse_Success_ThenSuccess()
    {
        var parser = If(_successParser, Any());

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be('b');
        result.Consumed.Should().Be(2);
    }

    [Test]
    public void Parse_Fail_ThenSuccess()
    {
        var parser = If(_failParser, Any());

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
    }

    [Test]
    public void Parse_Success_ThenFail()
    {
        var parser = If(_successParser, _failParser, Produce(() => true));

        var input = FromString("abc");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        input.Peek().Should().Be('a');
    }

    [Test]
    public void GetChildren_Test()
    {
        var c = Produce(() => true);
        var parser = If(_successParser, _failParser, c);
        var results = parser.GetChildren().ToList();
        results.Count.Should().Be(3);
        results.Should().Contain(_successParser);
        results.Should().Contain(_failParser);
        results.Should().Contain(c);
    }

    [Test]
    public void Parse_BracketingExample()
    {
        // We want to parse out a list of atoms, which are either a single letter or
        // a bracketed letter "[a]", etc. If() selects the bracketing parser if required
        var any = Any().Transform(c => c.ToString());
        var bracketed = If(
            None(
                And(
                    Match('['),
                    Any()
                )
            ),
            Rule(
                Match('['),
                any,
                Match(']'),
                (o, a, c) => $"{o}{a}{c}"
            )
        );
        var parser = First(
            bracketed,
            any
        ).List();
        var input = FromString("ab[c]d");
        var result = parser.Parse(input).Value.ToList();
        result[0].Should().Be("a");
        result[1].Should().Be("b");
        result[2].Should().Be("[c]");
        result[3].Should().Be("d");
    }
}
