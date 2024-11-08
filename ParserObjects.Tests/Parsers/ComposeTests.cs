using System.Linq;
using static ParserObjects.Sequences;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public class ComposeTests
{
    [Test]
    public void Parse_Test()
    {
        var inner = Parsers<byte>.Any().Transform(b => (char)b);
        var outer = CharacterString("abc");
        var target = Compose(inner, outer);

        var input = FromList(new byte[] { 97, 98, 99 });
        var result = target.Parse(input);
        result.Success.Should().BeTrue();
        result.Value.Should().Be("abc");
    }

    [Test]
    public void Parse_OuterFail()
    {
        var inner = Parsers<byte>.Any().Transform(b => (char)b);
        var outer = CharacterString("XYZ");
        var target = Compose(inner, outer);

        var input = FromList(new byte[] { 97, 98, 99 });
        var result = target.Parse(input);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_InnerFail()
    {
        var inner = Parsers<byte>.Match(b => b >= 97 && b <= 99).Transform(b => (char)b);
        var outer = CharacterString("abc");
        var target = Compose(inner, outer);

        var input = FromList(new byte[] { 10, 11, 12 });
        var result = target.Parse(input);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Parse_Empty()
    {
        var inner = Parsers<byte>.Any().Transform(b => (char)b);
        var outer = CharacterString("abc");
        var target = Compose(inner, outer);

        var input = FromList(Array.Empty<byte>());
        var result = target.Parse(input);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Match_Test()
    {
        var inner = Parsers<byte>.Any().Transform(b => (char)b);
        var outer = CharacterString("abc");
        var target = Compose(inner, outer);

        var input = FromList(new byte[] { 97, 98, 99 });
        var result = target.Match(input);
        result.Should().BeTrue();
    }

    [Test]
    public void GetChildren_Test()
    {
        var inner = Parsers<byte>.Any().Transform(b => (char)b);
        var outer = CharacterString("abc");
        var target = Compose(inner, outer);

        var result = target.GetChildren().ToList();
        result.Count.Should().Be(2);
        result.Should().Contain(inner);
        result.Should().Contain(outer);
    }

    [Test]
    public void ToBnf_Test()
    {
        var inner = Parsers<byte>.Any().Transform(b => (char)b);
        var outer = CharacterString("abc");
        var target = Compose(inner, outer).Named("target");
        var result = target.ToBnf();
        result.Should().Contain("target := . * 'a' 'b' 'c'");
    }
}
