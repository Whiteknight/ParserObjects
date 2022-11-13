using System.Linq;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers
{
    public class FunctionTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Function<object>(args => args.Success($"ok:{args.Input.GetNext()}", args.Input.CurrentLocation));
            var result = parser.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ok:X");
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Fail()
        {
            var parser = Function<object>(args => args.Failure(""));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Exception()
        {
            var parser = Function<object>(args => throw new System.Exception("fail"));
            Action act = () => parser.Parse("X");
            act.Should().Throw<Exception>();
        }

        [Test]
        public void Parse_Full_Success()
        {
            var parser = Function<object>(args => args.Success($"ok:{args.Input.GetNext()}"));
            var result = parser.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ok:X");
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Full_Fail()
        {
            var parser = Function<object>(args => args.Failure(""));
            var result = parser.Parse("X");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren()
        {
            var parser = Function<object>(args => args.Success($"ok:{args.Input.GetNext()}", args.Input.CurrentLocation));
            var children = parser.GetChildren().ToList();
            children.Count.Should().Be(0);
        }
    }
}
