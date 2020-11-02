using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ChooseParserTests
    {
        [Test]
        public void Parse_Basic()
        {
            var parser = Any().Choose(c =>
            {
                if (c == 'a')
                    return Match("aX");
                if (c == 'b')
                    return Match("bY");
                return Match(new[] { c, char.ToUpper(c) });
            }).Transform(x => $"{x[0]}{x[1]}");
            var result = parser.Parse("aX");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("aX");

            result = parser.Parse("bY");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("bY");

            result = parser.Parse("aY");
            result.Success.Should().BeFalse();

            result = parser.Parse("bX");
            result.Success.Should().BeFalse();

            result = parser.Parse("cC");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("cC");

        }
    }
}
