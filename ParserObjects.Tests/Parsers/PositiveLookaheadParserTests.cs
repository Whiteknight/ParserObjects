using System.Linq;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.SequenceMethods;

namespace ParserObjects.Tests.Parsers
{
    public class PositiveLookaheadParserTests
    {
        [Test]
        public void Parse_Fail()
        {
            var failParser = Fail<char>();
            var parser = PositiveLookahead(failParser);

            var input = FromString("abc");
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
