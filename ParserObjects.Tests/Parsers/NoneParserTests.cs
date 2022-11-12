using static ParserObjects.ParserMethods<char>;
using static ParserObjects.SequenceMethods;

namespace ParserObjects.Tests.Parsers
{
    public class NoneParserTests
    {
        // TODO: Need tests for the None() ParserMethod, not just the extension

        [Test]
        public void Parse_Output_Test()
        {
            var target = Any().None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Output_Fail()
        {
            var target = Fail<char>().None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Test()
        {
            var target = And(Any(), Any()).None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = And(Any(), Fail<char>()).None();
            var input = FromString("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Any().None().Named("target");
            var result = target.ToBnf();
            result.Should().Contain("target := (?=.)");
        }
    }
}
