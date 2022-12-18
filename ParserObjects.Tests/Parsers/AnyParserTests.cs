using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class AnyParserTests
    {
        [Test]
        public void Any_Test()
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
        public void AnyEnd_Test()
        {
            var parser = Any().FollowedBy(End());
            parser.Match("a").Should().BeTrue();
            parser.Match("ab").Should().BeFalse();
        }
    }
}
