using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers.Logical;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.Logical.ParserMethods<char>;
using static ParserObjects.Parsers.ParserMethods<char>;

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
            var result = parser.ParseUntyped(input);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void NotExtension_Parse_Fail()
        {
            var parser = _failParser.Not();

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