using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class TransformParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Transform(
                Any<char>(),
                c => int.Parse(c.ToString())
            );
            parser.Parse("1").Value.Should().Be(1);
        }
    }
}
