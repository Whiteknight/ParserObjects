using System.Linq;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class EmptyParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Empty();
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Test]
        public void Parse_End()
        {
            var parser = Empty();
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Empty();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
