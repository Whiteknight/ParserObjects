namespace ParserObjects.Tests.Parsers;

using static ParserObjects.Parsers<char>;

public class TransformResultParserTests
{
    [Test]
    public void Parse_Test()
    {
        var any = Any();
        var parser = TransformResult<char, int>(
            any,
            args => args.Result.Transform(s => int.Parse(s.ToString()))
        );
        var result = parser.Parse("1");
        result.Value.Should().Be(1);
        result.Consumed.Should().Be(1);
        result.Parser.Should().BeSameAs(any);
    }

    [Test]
    public void Parse_InnerFail()
    {
        var fail = Fail<char>();
        var parser = TransformResult<char, int>(
            fail,
            args => args.Result.Transform(s => int.Parse(s.ToString()))
        );
        var result = parser.Parse("1");
        result.Success.Should().BeFalse();
        result.Consumed.Should().Be(0);
        result.Parser.Should().BeSameAs(fail);
    }

    [Test]
    public void Parse_TransformFail()
    {
        var any = Any();
        var parser = TransformResult<char, int>(
            any,
            args => args.Failure("fail")
        );
        var result = parser.Parse("1");
        result.Consumed.Should().Be(0);
        result.Parser.Should().BeSameAs(parser);
    }
}
