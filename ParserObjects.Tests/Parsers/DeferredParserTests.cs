using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class DeferredParserTests
    {
        [Test]
        public void Parse_Any()
        {
            var target = Deferred(() => Any<char>());
            var input = new StringCharacterSequence("abc");
            target.Parse(input).Value.Should().Be('a');
            target.Parse(input).Value.Should().Be('b');
            target.Parse(input).Value.Should().Be('c');
            target.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any<char>();
            var target = Deferred(() => anyParser);
            var result = target.GetChildren().ToList();
            result.Count().Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var anyParser = Any<char>();
            var failParser = Fail<char, char>();
            var target = Deferred(() => failParser);
            target = target.ReplaceChild(failParser, anyParser) as IParser<char, char>;

            var input = new StringCharacterSequence("abc");
            target.Parse(input).Value.Should().Be('a');
            target.Parse(input).Value.Should().Be('b');
            target.Parse(input).Value.Should().Be('c');
            target.Parse(input).Success.Should().BeFalse();
        }
    }
}
