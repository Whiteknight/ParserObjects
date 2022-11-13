using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers
{
    public class EndParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = End();
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
