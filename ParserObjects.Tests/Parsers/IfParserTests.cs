using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class IfParserTests
    {
        private readonly IParser<char, char> _successParser = Any();
        private readonly IParser<char, bool> _failParser = Fail<bool>();

        [Test]
        public void ExtThen_Success_ThenSuccess()
        {
            var parser = _successParser.Then(Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('b');
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void ExtThen_Fail_ThenSuccess()
        {
            var parser = _failParser.Then(Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void ExtIf_Success_ThenSuccess()
        {
            var parser = Any().If(_successParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('b');
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void ExtIf_Fail_ThenSuccess()
        {
            var parser = Any().If(_failParser);

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Success_ThenSuccess()
        {
            var parser = If(_successParser, Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be('b');
            result.Consumed.Should().Be(2);
        }

        [Test]
        public void Parse_Fail_ThenSuccess()
        {
            var parser = If(_failParser, Any());

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Parse_Success_ThenFail()
        {
            var parser = If(_successParser, _failParser, Produce(() => true));

            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            input.Peek().Should().Be('a');
        }

        [Test]
        public void GetChildren_Test()
        {
            var c = Produce(() => true);
            var parser = If(_successParser, _failParser, c);
            var results = parser.GetChildren().ToList();
            results.Count.Should().Be(3);
            results.Should().Contain(_successParser);
            results.Should().Contain(_failParser);
            results.Should().Contain(c);
        }
    }
}
