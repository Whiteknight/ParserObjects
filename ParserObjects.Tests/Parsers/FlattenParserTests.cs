using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class FlattenParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Produce(() => "abc").Flatten<char, IEnumerable<char>, char>();
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Value.Should().Be('a');

            result = parser.Parse(input);
            result.Value.Should().Be('b');

            result = parser.Parse(input);
            result.Value.Should().Be('c');
        }

        [Test]
        public void ParseUntyped_Test()
        {
            var parser = Produce(() => "abc").Flatten<char, IEnumerable<char>, char>();
            var input = new StringCharacterSequence("");
            var result = parser.ParseUntyped(input);
            result.Value.Should().Be('a');

            result = parser.ParseUntyped(input);
            result.Value.Should().Be('b');

            result = parser.ParseUntyped(input);
            result.Value.Should().Be('c');
        }

        [Test]
        public void Parse_Fail()
        {
            var parser =  Fail<IEnumerable<char>>().Flatten<char, IEnumerable<char>, char>();
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Empty()
        {
            var parser = Produce(() => "").Flatten<char, IEnumerable<char>, char>();
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var produceParser = Produce(() => "abc");
            var failParser = Fail<IEnumerable<char>>();

            var parser = failParser.Flatten<char, IEnumerable<char>, char>();
            parser = parser.ReplaceChild(failParser, produceParser) as IParser<char, char>;
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Value.Should().Be('a');

            result = parser.Parse(input);
            result.Value.Should().Be('b');

            result = parser.Parse(input);
            result.Value.Should().Be('c');
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var produceParser = Produce(() => "abc");
            var parser = produceParser.Flatten<char, IEnumerable<char>, char>();
            var result  = parser.ReplaceChild(null, null) as IParser<char, char>;
            result.Should().BeSameAs(parser);
        }

        [Test]
        public void GetChildren_Same()
        {
            var produceParser = Produce(() => "abc");
            var parser = produceParser.Flatten<char, IEnumerable<char>, char>();
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(produceParser);
        }
    }
}
