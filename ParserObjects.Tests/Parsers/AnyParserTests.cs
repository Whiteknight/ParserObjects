using System.Linq;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class AnyParserTests
    {
        [Test]
        public void Any_Test()
        {
            var target = Any();
            var input = new StringCharacterSequence("abc");
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
            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("ab").Should().BeFalse();
        }
    }
}
