using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class PositiveLookaheadParserTests
    {
        [Test]
        public void FollowedBy_Fail()
        {
            var parser = Match('[').FollowedBy(Match('~'));
            var input = new StringCharacterSequence("[test]");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('[');
        }

        [Test]
        public void FollowedBy_Success()
        {
            var parser = Match('[').FollowedBy(Match('~'));
            var input = new StringCharacterSequence("[~test]");
            var result = parser.Parse(input);
            result.Value.Should().Be('[');
            result.Consumed.Should().Be(1);
            input.Peek().Should().Be('~');
        }

        [Test]
        public void Parse_Fail()
        {
            var failParser = Fail<char>();
            var parser = PositiveLookahead(failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char>();
            var parser = PositiveLookahead(failParser);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(failParser);
        }
    }
}
