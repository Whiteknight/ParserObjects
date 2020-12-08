using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class NoneParserTests
    {
        [Test]
        public void Parse_Output_Test()
        {
            var target = Any().None();
            var input = new StringCharacterSequence("abc");
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
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Test()
        {
            var target = And(Any(), Any()).None();
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }

        [Test]
        public void Parse_Fail()
        {
            var target = And(Any(), Fail<char>()).None();
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            input.Peek().Should().Be('a');
        }
    }
}
