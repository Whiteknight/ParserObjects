using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class NegativeLookaheadParserTests
    {
        [Test]
        public void Parse_Fail()
        {
            var failParser = Fail<char>();
            var parser = NegativeLookahead(failParser);

            var input = FromString("abc");
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
