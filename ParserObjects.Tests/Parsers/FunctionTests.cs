using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

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
        public void Parse_Null()
        {
            var parser = Function<object>(args => null);
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

        // Function() takes two callbacks: The first one, required, handles parse. The second one,
        // optional, handles match. If the match() callback is not provided, the Function parser
        // falls back to call the parse() callback instead. "_Fallback" methods test that case.

        [Test]
        public void Match_Test()
        {
            var parser = Function<object>(args => args.Success($"ok:{args.Input.GetNext()}", args.Input.CurrentLocation), args =>
            {
                args.Input.GetNext();
                return true;
            });
            var input = FromString("X");
            var result = parser.Match(input);
            result.Should().BeTrue();
            input.Consumed.Should().Be(1);
        }

        [Test]
        public void Match_Test_Fallback()
        {
            var parser = Function<object>(args => args.Success($"ok:{args.Input.GetNext()}", args.Input.CurrentLocation));
            var input = FromString("X");
            var result = parser.Match(input);
            result.Should().BeTrue();
            input.Consumed.Should().Be(1);
        }

        [Test]
        public void Match_Fail()
        {
            var parser = Function<object>(args => args.Failure(""), _ => false);
            var input = FromString("X");
            var result = parser.Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_Fail_Fallback()
        {
            var parser = Function<object>(args => args.Failure(""));
            var input = FromString("X");
            var result = parser.Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void Match_Null_Fallback()
        {
            var parser = Function<object>(args => null);
            var input = FromString("X");
            var result = parser.Match(input);
            result.Should().BeFalse();
            input.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren()
        {
            var parser = Function<object>(args => args.Success($"ok:{args.Input.GetNext()}", args.Input.CurrentLocation));
            var children = parser.GetChildren().ToList();
            children.Count.Should().Be(0);
        }

        [Test]
        public void ToBnf_Test()
        {
            var parser = Function<string>(args => args.Success("")).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := User Function");
        }

        [Test]
        public void ToBnf_SimpleDescription()
        {
            var parser = Function<string>(args => args.Success(""), description: "TEST").Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := TEST");
        }
    }
}
