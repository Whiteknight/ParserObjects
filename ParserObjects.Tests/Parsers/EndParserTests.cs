using System.Linq;
using ParserObjects.Parsers;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class EndParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = new EndParser<char>();
            parser.CanMatch("").Should().BeTrue();
            parser.CanMatch("x").Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = End();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
