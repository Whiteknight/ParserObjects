using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class FirstParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = new FirstParser<char, char>(
                Match('a'),
                Match('X'),
                Match('1')
            );

            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("X").Should().BeTrue();
            parser.CanMatch("1").Should().BeTrue();

            parser.CanMatch("b").Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var aParser = Match('a');
            var XParser = Match('X');
            var oneParser = Match('1');
            var bParser = Match('b');
            var parser = First(
                aParser,
                XParser,
                oneParser
            );
            var results = parser.GetChildren().ToList();
            results.Count.Should().Be(3);
            results[0].Should().Be(aParser);
            results[1].Should().Be(XParser);
            results[2].Should().Be(oneParser);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var aParser = Match('a');
            var XParser = Match('X');
            var oneParser = Match('1');
            var bParser = Match('b');
            var parser = First(
                aParser,
                XParser,
                oneParser
            );
            parser = parser.ReplaceChild(XParser, bParser) as IParser<char, char>;

            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("X").Should().BeFalse();
            parser.CanMatch("1").Should().BeTrue();

            parser.CanMatch("b").Should().BeTrue();
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var aParser = Match('a');
            var XParser = Match('X');
            var oneParser = Match('1');
            var bParser = Match('b');
            var parser = First(
                aParser,
                XParser,
                oneParser
            );
            var result = parser.ReplaceChild(null, null) as IParser<char, char>;
            result.Should().BeSameAs(parser);
        }
    }
}