using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class RuleParserTests
    {
        // TODO: We need a lot of tests for various Rule() variants
        [Test]
        public void Rule_2_Test()
        {
            var anyParser = Any<char>();

            var target = Rule(
                anyParser,
                anyParser,

                (a, b) => $"{a}{b}"
            );

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Should().Be("ab");
        }

        // TODO: We need tests for various Tuple.Produce() variants
        [Test]
        public void ValueTuple_Produce_2_Test()
        {
            var anyParser = Any<char>();

            var target = (anyParser, anyParser).Produce((a, b) => $"{a}{b}");

            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Should().Be("ab");
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var anyParser = Any<char>();
            var failParser = Fail<char, char>();

            var target = Rule(
                anyParser,
                failParser,

                (a, b) => $"{a}{b}"
            );

            target = target.ReplaceChild(failParser, anyParser) as IParser<char, string>;
            var input = new StringCharacterSequence("abc");

            target.Parse(input).Value.Should().Be("ab");
        }

        [Test]
        public void ReplaceChild_NotFound()
        {
            var anyParser = Any<char>();
            var failParser = Fail<char, char>();

            var target = Rule(
                anyParser,
                failParser,

                (a, b) => $"{a}{b}"
            );

            var result = target.ReplaceChild(null, null);
            result.Should().BeSameAs(target);
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any<char>();
            var failParser = Fail<char, char>();

            var target = Rule(
                anyParser,
                failParser,

                (a, b) => $"{a}{b}"
            );

            var result = target.GetChildren().ToList();
            result[0].Should().BeSameAs(anyParser);
            result[1].Should().BeSameAs(failParser);
        }
    }
}
