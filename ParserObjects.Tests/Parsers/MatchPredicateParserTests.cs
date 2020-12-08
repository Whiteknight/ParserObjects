using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class MatchPredicateParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Match(char.IsNumber);
            var input = new StringCharacterSequence("123");
            var result = parser.Parse(input);
            result.Value.Should().Be('1');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Match(char.IsLetter);
            var input = new StringCharacterSequence("123");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Match(char.IsNumber);
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
