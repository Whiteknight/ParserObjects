using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class AnyTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = Any();
            var input = FromString("abc");
            target.Parse(input).Value.Should().Be('a');
            target.Parse(input).Value.Should().Be('b');
            target.Parse(input).Value.Should().Be('c');
            target.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = Any();
            target.GetChildren().Count().Should().Be(0);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Any().Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }
    }
}
