using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class NotTests
{
    private static readonly IParser<char, char> _anyParser = Any();
    private static readonly IParser<char, bool> _failParser = Fail<bool>();

    public class Method
    {
        [Test]
        public void Parse_Fail()
        {
            var parser = Not(_failParser);

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Success()
        {
            var parser = Not(_anyParser);

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_Fail()
        {
            var parser = Not(_failParser);

            var input = FromString("abc");
            var result = parser.Match(input);
            result.Should().BeTrue();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_Success()
        {
            var parser = Not(_anyParser);

            var input = FromString("abc");
            var result = parser.Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Not(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := !.");
        }
    }

    public class Extension
    {
        [Test]
        public void Parse_Fail()
        {
            var parser = _failParser.Not();

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Success()
        {
            var parser = _anyParser.Not();

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_Fail()
        {
            var parser = _failParser.Not();

            var input = FromString("abc");
            var result = parser.Match(input);
            result.Should().BeTrue();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_Success()
        {
            var parser = _anyParser.Not();

            var input = FromString("abc");
            var result = parser.Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }
    }
}
