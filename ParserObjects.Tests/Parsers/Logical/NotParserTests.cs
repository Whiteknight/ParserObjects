using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class NotParserTests
    {
        [Test]
        public void Parse_False()
        {
            var parser = ParserObjects.Parsers.Logical.ParserMethods.Not(ParserMethods.Produce<char, bool>(() => false));

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Test]
        public void Parse_True()
        {
            var parser = ParserObjects.Parsers.Logical.ParserMethods.Not(ParserMethods.Produce<char, bool>(() => true));

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = ParserObjects.Parsers.Logical.ParserMethods.Not(ParserMethods.Fail<char, bool>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }
    }
}