using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers.Specialty;

namespace ParserObjects.Tests.Parsers.Specialty
{
    public class WhitespaceParserMethodsTests
    {
        [Test]
        public void Whitespace_Tests()
        {
            var parser = WhitespaceParsersMethods.Whitespace();
            parser.CanMatch("").Should().BeFalse();
            parser.CanMatch(" ").Should().BeTrue();
            parser.CanMatch("\t").Should().BeTrue();
            parser.CanMatch("\r").Should().BeTrue();
            parser.CanMatch("\n").Should().BeTrue();
            parser.CanMatch("\v").Should().BeTrue();
            parser.CanMatch("x").Should().BeFalse();
        }
    }
}
