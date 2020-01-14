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
    }
}
