using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class SequentialParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Sequential(s =>
            {
                var first = s.Parse(Any());
                char second;
                if (first == 'a')
                    second = s.Parse(Match('b'));
                else
                    second = s.Parse(Match('y'));
                var third = s.Parse(Match('c'));

                return $"{first}{second}{third}".ToUpper();
            });

            var result = parser.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("ABC");

            result = parser.Parse("xyc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("XYC");
        }
    }
}
