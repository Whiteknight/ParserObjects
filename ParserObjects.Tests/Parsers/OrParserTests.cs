using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class OrParserTests
    {
        private readonly IParser<char, char> _anyParser = Any();
        private readonly IParser<char, char> _failParser = Fail<char>();

        [Test]
        public void Parse_Success_Success()
        {
            var parser = Or(_anyParser, _anyParser);

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Extension_Success_Success()
        {
            var parser = _anyParser.Or(_anyParser);

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Success_Fail()
        {
            var parser = Or(_anyParser, _failParser);

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Fail_Success()
        {
            var parser = Or(_failParser, _anyParser);

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Fail_Fail()
        {
            var parser = Or(_failParser, _failParser);

            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }
}
