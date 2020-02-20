using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class RuleParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var anyParser = Any<char>();

            var target = Rule(
                anyParser,
                anyParser,

                (a, b) => a.ToString() + b.ToString()
            );

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

                (a, b) => a.ToString() + b.ToString()
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

                (a, b) => a.ToString() + b.ToString()
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

                (a, b) => a.ToString() + b.ToString()
            );

            var result = target.GetChildren().ToList();
            result[0].Should().BeSameAs(anyParser);
            result[1].Should().BeSameAs(failParser);
        }
    }
}
