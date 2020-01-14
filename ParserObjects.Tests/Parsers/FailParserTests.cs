using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers
{
    public class FailParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = new FailParser<char, object>();
            parser.CanMatch("").Should().BeFalse();
            parser.CanMatch(" ").Should().BeFalse();
            parser.CanMatch("1").Should().BeFalse();
            parser.CanMatch("x").Should().BeFalse();
        }
    }
}
