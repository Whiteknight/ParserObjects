using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class EndParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = new EndParser<char>();
            parser.CanMatch("").Should().BeTrue();
            parser.CanMatch("x").Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = End<char>();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
