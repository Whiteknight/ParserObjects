using System.Linq;
using static ParserObjects.ParserMethods<char>;
using static ParserObjects.SequenceMethods;

namespace ParserObjects.Tests.Parsers
{
    public class PeekParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = Peek();
            var input = FromString("abc");
            target.Parse(input).Value.Should().Be('a');
            target.Parse(input).Value.Should().Be('a');
            target.Parse(input).Value.Should().Be('a');
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = Peek();
            target.GetChildren().Count().Should().Be(0);
        }
    }
}
