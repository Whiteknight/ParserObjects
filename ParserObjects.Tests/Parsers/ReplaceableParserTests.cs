using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ReplaceableParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
        }

        [Test]
        public void ParseUntyped_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.ParseUntyped(input);
            result.Value.Should().Be('a');
        }

        [Test]
        public void SetParser_Test()
        {
            var anyParser = Any();
            var failParser = Fail<char>();
            var target = new ReplaceableParser<char, char>(failParser);
            target.SetParser(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var anyParser = Any();
            var failParser = Fail<char>();
            var target = Replaceable(failParser);
            target = target.ReplaceChild(failParser, anyParser) as IParser<char, char>;
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }
    }
}
