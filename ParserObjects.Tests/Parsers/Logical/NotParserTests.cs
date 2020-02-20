using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Logical.ParserMethods;


namespace ParserObjects.Tests.Parsers.Logical
{
    public class NotParserTests
    {
        private readonly IParser<char, bool> trueParser = Produce<char, bool>(() => true);
        private readonly IParser<char, bool> falseParser = Produce<char, bool>(() => false);
        private readonly IParser<char, bool> failParser = Fail<char, bool>();

        [Test]
        public void Parse_False()
        {
            var parser = Not(falseParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_True()
        {
            var parser = Not(trueParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Not(failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_ReplaceChild()
        {
            var parser = Not(trueParser);
            parser = parser.ReplaceChild(trueParser, falseParser) as IParser<char, bool>;

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }
    }
}