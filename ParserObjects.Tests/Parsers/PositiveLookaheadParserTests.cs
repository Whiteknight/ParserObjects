using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class PositiveLookaheadParserTests
    {
        [Test]
        public void FollowedBy_Fail()
        {
            var parser = Match('[').FollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[test]");
            parser.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void FollowedBy_Success()
        {
            var parser = Match('[').FollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[~test]");
            parser.Parse(input).Value.Should().Be('[');
        }

        [Test]
        public void Parse_Fail()
        {
            var failParser = Fail<char, char>();
            var parser = PositiveLookahead(failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var failParser = Fail<char, char>();
            var anyParser = Any<char>();
            var parser = PositiveLookahead(failParser);
            parser = parser.ReplaceChild(failParser, anyParser) as IParser<char, object>;

            var input = new StringCharacterSequence("abc");
            parser.Parse(input).Success.Should().Be(true);
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var failParser = Fail<char, char>();
            var parser = PositiveLookahead(failParser);
            var result = parser.ReplaceChild(null, null) as IParser<char, object>;

            result.Should().BeSameAs(parser);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char, char>();
            var parser = PositiveLookahead(failParser);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(failParser);
        }
    }
}