using System.Linq;
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
            var result = parser.Parse(input);
            result.Value.Should().Be('[');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void NotFollowedBy_Fail()
        {
            var parser = Match('[').NotFollowedBy(Match("~"));
            var input = new StringCharacterSequence("[~test]");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Fail()
        {
            var failParser = Fail<char>();
            var parser = NegativeLookahead(failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().Be(true);
            result.Consumed.Should().Be(0);
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
