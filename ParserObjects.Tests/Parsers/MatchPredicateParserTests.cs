using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class MatchPredicateParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Match<char>(char.IsNumber);
            var input = new StringCharacterSequence("123");
            parser.Parse(input).Value.Should().Be('1');
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Match<char>(char.IsLetter);
            var input = new StringCharacterSequence("123");
            parser.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Match<char>(char.IsNumber);
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
