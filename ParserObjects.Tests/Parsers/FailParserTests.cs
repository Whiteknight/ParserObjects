using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class FailParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = new Fail<char, object>.Parser();
            parser.CanMatch("").Should().BeFalse();
            parser.CanMatch(" ").Should().BeFalse();
            parser.CanMatch("1").Should().BeFalse();
            parser.CanMatch("x").Should().BeFalse();
        }

        [Test]
        public void Parse_Multi()
        {
            var parser = new Fail<char, object>.MultiParser();
            parser.CanMatch("").Should().BeFalse();
            parser.CanMatch(" ").Should().BeFalse();
            parser.CanMatch("1").Should().BeFalse();
            parser.CanMatch("x").Should().BeFalse();
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Fail<object>();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
