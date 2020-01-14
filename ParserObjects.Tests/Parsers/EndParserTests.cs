using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;

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
    }
}
