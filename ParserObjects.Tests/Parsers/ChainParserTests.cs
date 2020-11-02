using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ChainParserTests
    {
        [Test]
        public void Parse_Basic()
        {
            var parser = Any().Chain(c =>
            {
                if (c == 'a')
                    return Match('X');
                if (c == 'b')
                    return Match('Y');
                return Match('Z');
            });
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Y');

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();

            result = parser.Parse("cZ");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('Z');

        }
    }
}
