using System.Linq;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers;

internal class CombineTests
{
    private readonly IParser<char, char> _any = Any();

    [Test]
    public void GetChildren_Test()
    {
        var failParser = Fail<char>();

        var target = (_any, failParser).Combine();

        var result = target.GetChildren().ToList();
        result[0].Should().BeSameAs(_any);
        result[1].Should().BeSameAs(failParser);
    }

    [Test]
    public void RewindInput_Test()
    {
        var parser = (Match('a'), Match('b'), Match('c')).Combine();
        var input = new StringCharacterSequence("abd");
        var result = parser.Parse(input);
        result.Success.Should().BeFalse();
        input.Peek().Should().Be('a');
    }

    [Test]
    public void ValueTuple_Combine_2_Test()
    {
        var target = (_any, _any).Combine();

        var input = new StringCharacterSequence("abc");

        target.Parse(input).Value.Count.Should().Be(2);
    }

    [Test]
    public void ValueTuple_Combine_3_Test()
    {
        var target = (_any, _any, _any).Combine();

        var input = new StringCharacterSequence("abc");

        target.Parse(input).Value.Count.Should().Be(3);
    }

    [Test]
    public void ValueTuple_Combine_4_Test()
    {
        var target = (_any, _any, _any, _any).Combine();

        var input = new StringCharacterSequence("abcdefghi");

        target.Parse(input).Value.Count.Should().Be(4);
    }

    [Test]
    public void ValueTuple_Combine_5_Test()
    {
        var target = (_any, _any, _any, _any, _any).Combine();

        var input = new StringCharacterSequence("abcdefghi");

        target.Parse(input).Value.Count.Should().Be(5);
    }

    [Test]
    public void ValueTuple_Combine_6_Test()
    {
        var target = (_any, _any, _any, _any, _any, _any).Combine();

        var input = new StringCharacterSequence("abcdefghi");

        target.Parse(input).Value.Count.Should().Be(6);
    }

    [Test]
    public void ValueTuple_Combine_7_Test()
    {
        var target = (_any, _any, _any, _any, _any, _any, _any).Combine();

        var input = new StringCharacterSequence("abcdefghi");

        target.Parse(input).Value.Count.Should().Be(7);
    }

    [Test]
    public void ValueTuple_Combine_8_Test()
    {
        var target = (_any, _any, _any, _any, _any, _any, _any, _any).Combine();

        var input = new StringCharacterSequence("abcdefghi");

        target.Parse(input).Value.Count.Should().Be(8);
    }

    [Test]
    public void ValueTuple_Combine_9_Test()
    {
        var target = (_any, _any, _any, _any, _any, _any, _any, _any, _any).Combine();

        var input = new StringCharacterSequence("abcdefghi");

        var result = target.Parse(input).Value;
        result.Count.Should().Be(9);
    }
}
