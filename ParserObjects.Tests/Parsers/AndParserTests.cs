using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class AndParserTests
    {
        private readonly IParser<char, char> _anyParser = Any();
        private readonly IParser<char, char> _failParser = Fail<char>();

        [Test]
        public void Parse_Success_Success()
        {
            var parser = And(_anyParser, _anyParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void AndExtension_Parse_Success_Success()
        {
            // Same as above, just different syntax
            var parser = _anyParser.And(_anyParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void Parse_Success_Fail()
        {
            var parser = And(_anyParser, _failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_Success()
        {
            var parser = And(_failParser, _anyParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_Fail()
        {
            var parser = And(_failParser, _failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_ReplaceChild_1()
        {
            var parser = And(_failParser, _anyParser);
            parser = parser.ReplaceChild(_failParser, _anyParser) as IParser<char>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
        }

        [Test]
        public void Parse_ReplaceChild_2()
        {
            var parser = And(_anyParser, _failParser);
            parser = parser.ReplaceChild(_failParser, _anyParser) as IParser<char>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
        }
    }
}