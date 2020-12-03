using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class MatchSequenceParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Match("abc");
            var input = new StringCharacterSequence("abcd");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count.Should().Be(3);
            result.Value[0].Should().Be('a');
            result.Value[1].Should().Be('b');
            result.Value[2].Should().Be('c');
        }

        [Test]
        public void Parse_Empty()
        {
            var parser = Match("");
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Match("abc");
            var input = new StringCharacterSequence("abcd");
            parser.GetChildren().Count().Should().Be(0);
        }

        [Test]
        public void CharacterString_Test()
        {
            var parser = ParserMethods.CharacterString("abc");
            var input = new StringCharacterSequence("abcd");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abc");
        }
    }
}
