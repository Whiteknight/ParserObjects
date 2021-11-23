using System.Linq;
using ParserObjects.Parsers;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class FailParserTests
    {
        private IParser<char, char> SingleInstance() => new FailParser<char, char>();

        private IMultiParser<char, char> MultiInstance() => new FailParser<char, char>();

        [Test]
        public void Parse_Test()
        {
            var parser = SingleInstance();
            parser.CanMatch("").Should().BeFalse();
            parser.CanMatch(" ").Should().BeFalse();
            parser.CanMatch("1").Should().BeFalse();
            parser.CanMatch("x").Should().BeFalse();
        }

        [Test]
        public void Parse_Multi()
        {
            var parser = MultiInstance();
            parser.CanMatch("").Should().BeFalse();
            parser.CanMatch(" ").Should().BeFalse();
            parser.CanMatch("1").Should().BeFalse();
            parser.CanMatch("x").Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Fail<object>();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
