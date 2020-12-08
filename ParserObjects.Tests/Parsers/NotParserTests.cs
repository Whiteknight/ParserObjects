using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class NotParserTests
    {
        private readonly IParser<char, char> _anyParser = Any();
        private readonly IParser<char, bool> _failParser = Fail<bool>();

        [Test]
        public void Parse_Fail()
        {
            var parser = Not(_failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void NotExtension_Parse_Fail()
        {
            var parser = _failParser.Not();

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Success()
        {
            var parser = Not(_anyParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }
}
