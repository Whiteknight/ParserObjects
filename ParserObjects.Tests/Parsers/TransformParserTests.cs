using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class TransformParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Transform(
                Any(),
                c => int.Parse(c.ToString())
            );
            parser.Parse("1").Value.Should().Be(1);
        }

        [Test]
        public void Parse_Extension()
        {
            var parser = Any().Transform(c => int.Parse(c.ToString()));
            parser.Parse("1").Value.Should().Be(1);
        }

        [Test]
        public void Parse_Extension_Map()
        {
            var parser = Any().Map(c => int.Parse(c.ToString()));
            parser.Parse("1").Value.Should().Be(1);
        }

        [Test]
        public void Parse_Failure()
        {
            var parser = Transform(
                Fail<char>(),
                c => int.Parse(c.ToString())
            );
            parser.Parse("1").Success.Should().BeFalse();
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var failParser = Fail<char>();
            var parser = Transform(
                failParser,
                c => int.Parse(c.ToString())
            );
            parser = parser.ReplaceChild(failParser, Any()) as IParser<char, int>;
            parser.Parse("1").Value.Should().Be(1);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char>();
            var parser = Transform(
                failParser,
                c => int.Parse(c.ToString())
            );
            var results = parser.GetChildren().ToList();
            results.Count.Should().Be(1);
            results[0].Should().BeSameAs(failParser);
        }
    }
}
