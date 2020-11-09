using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class NegativeLookaheadParserTests
    {
        [Test]
        public void NotFollowedBy_Success()
        {
            var parser = Match('[').NotFollowedBy(Match("~"));
            var input = new StringCharacterSequence("[test]");
            parser.Parse(input).Value.Should().Be('[');
        }

        [Test]
        public void NotFollowedBy_Fail()
        {
            var parser = Match('[').NotFollowedBy(Match("~"));
            var input = new StringCharacterSequence("[~test]");
            parser.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail()
        {
            var failParser = Fail<char>();
            var parser = NegativeLookahead(failParser);

            var input = new StringCharacterSequence("abc");
            parser.Parse(input).Success.Should().Be(true);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var failParser = Fail<char>();
            var anyParser = Any();
            var parser = NegativeLookahead(failParser);
            parser = parser.ReplaceChild(failParser, anyParser) as IParser<char, object>;

            var input = new StringCharacterSequence("abc");
            parser.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var failParser = Fail<char>();
            var parser = NegativeLookahead(failParser);
            var result = parser.ReplaceChild(null, null) as IParser<char, object>;

            result.Should().BeSameAs(parser);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char>();
            var parser = NegativeLookahead(failParser);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(failParser);
        }
    }
}
