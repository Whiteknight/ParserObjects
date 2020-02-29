using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Logical.ParserMethods;

namespace ParserObjects.Tests.Parsers.Logical
{
    public class LogicalExamplesTests
    {
        [Test]
        public void Test1()
        {
            var any = Any<char>().Transform(c => c.ToString());
            var bracketed = If(
                And(Match('['), Any<char>()),
                Rule(Match('['), any, Match(']'), (o, a, c) => $"{o}{a}{c}")
            );
            var parser = First(
                bracketed,
                any
            ).List();
            var input = new StringCharacterSequence("ab[c]d");
            var result = parser.Parse(input).Value.ToList();
            result[0].Should().Be("a");
            result[1].Should().Be("b");
            result[2].Should().Be("[c]");
            result[3].Should().Be("d");
        }
    }
}
