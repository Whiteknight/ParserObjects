using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class ListParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = List(Any<char>());
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var list = result.Value.ToList();
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void Parse_None()
        {
            var parser = List(Match<char>(char.IsNumber));
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count().Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any<char>();
            var parser = List(anyParser);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var anyParser = Any<char>();
            var failParser = Fail<char, char>();
            var parser = List(failParser);
            parser = parser.ReplaceChild(failParser, anyParser) as IParser<char, IEnumerable<char>>;
            
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var list = result.Value.ToList();
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var parser = List(Any<char>());
            var result = parser.ReplaceChild(null, null) as IParser<char, IEnumerable<char>>;
            result.Should().BeSameAs(parser);
        }
    }
}
