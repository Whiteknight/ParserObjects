using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Parsers.Logical;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Logical.ParserMethods;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class IfParserTests
    {
        [Test]
        public void Ext_Then_Match()
        {
            var parser = new ProduceParser<char, bool>(t => true).Then(Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_Then_NotMatch()
        {
            var parser = new ProduceParser<char, bool>(t => false).Then(Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Ext_If_Match()
        {
            var parser = Any<char>().If(Produce<char, bool>(() => true));

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Ext_If_NotMatch()
        {
            var parser = Any<char>().If(Produce<char, bool>(() => false));

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void If_Match()
        {
            var parser = If(Produce<char, bool>(() => true), Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void If_NotMatch()
        {
            var parser = If(Produce<char, bool>(() => false), Any<char>());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }
    }
}
