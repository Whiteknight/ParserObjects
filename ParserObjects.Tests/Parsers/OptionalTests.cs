using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class OptionalTests
{
    public class MethodWithoutDefault
    {
        [Test]
        public void Parse_Test()
        {
            var target = Optional(MatchChar('a'));
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.GetValueOrDefault('x').Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Optional(Fail());
            var result = target.Parse("");
            result.Success.Should().Be(true);
            result.Value.GetValueOrDefault('x').Should().Be('x');
        }

        [Test]
        public void Match_Test()
        {
            var target = Optional(MatchChar('a'));
            var result = target.Match("abc");
            result.Should().Be(true);
        }

        [Test]
        public void Match_Fail()
        {
            var target = Optional(Fail());
            var result = target.Match("");
            result.Should().Be(true);
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Optional(Match('a')).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := 'a'?");
        }
    }

    public class MethodGetDefaultAction
    {
        [Test]
        public void Parse_Test()
        {
            var target = Optional(Match('a'), () => 'x');
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Optional(Fail(), () => 'x');
            var result = target.Parse("");
            result.Success.Should().Be(true);
            result.Value.Should().Be('x');
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Optional(Match('a'), () => 'x').Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := 'a'?");
        }
    }

    public class MethodGetDefaultFunction
    {
        [Test]
        public void Parse_Test()
        {
            var target = Optional(Match('a'), state => state.Input.Peek());
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Optional(Fail(), state => state.Input.Peek());
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.Should().Be('a');
        }
    }

    public class ExtensionWithoutDefault
    {
        [Test]
        public void Parse_Test()
        {
            var target = Match('a').Optional();
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.GetValueOrDefault('x').Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Fail().Optional();
            var result = target.Parse("");
            result.Success.Should().Be(true);
            result.Value.GetValueOrDefault('x').Should().Be('x');
        }
    }

    public class ExtensionGetDefaultAction
    {
        [Test]
        public void Parse_Test()
        {
            var target = Match('a').Optional(() => 'x');
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Fail().Optional(() => 'x');
            var result = target.Parse("");
            result.Success.Should().Be(true);
            result.Value.Should().Be('x');
        }
    }

    public class ExtensionGetDefaultFunction
    {
        [Test]
        public void Parse_Test()
        {
            var target = Match('a').Optional(state => state.Input.Peek());
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Fail().Optional(state => state.Input.Peek());
            var result = target.Parse("abc");
            result.Success.Should().Be(true);
            result.Value.Should().Be('a');
        }
    }
}
