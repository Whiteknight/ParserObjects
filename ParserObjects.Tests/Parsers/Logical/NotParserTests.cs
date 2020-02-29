using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Logical.ParserMethods;


namespace ParserObjects.Tests.Parsers.Logical
{
    public class NotParserTests
    {
        private readonly IParser<char, char> _anyParser = Any<char>();
        private readonly IParser<char, bool> _failParser = Fail<char, bool>();

        [Test]
        public void Parse_Fail()
        {
            var parser = Not(_failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.ParseUntyped(input);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void Parse_Success()
        {
            var parser = Not(_anyParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.ParseUntyped(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_ReplaceChild()
        {
            var parser = Not(_anyParser);
            parser = parser.ReplaceChild(_anyParser, _failParser) as IParser<char, object>;

            var input = new StringCharacterSequence("abc");
            var result = parser.ParseUntyped(input);
            result.Success.Should().BeTrue();
        }
    }
}