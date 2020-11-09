using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods;

namespace ParserObjects.Tests
{
    public class QuotedParserMethods
    {
        [Test]
        public void DoubleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = DoubleQuotedString();
            var result = parser.Parse("\"TEST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"TEST\"");

            result = parser.Parse("\"TE\\\"ST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"TE\\\"ST\"");
        }

        [Test]
        public void SingleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = SingleQuotedString();
            var result = parser.Parse("'TEST'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'TEST'");
        }

        [Test]
        public void StrippedDoubleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = StrippedDoubleQuotedString();
            var result = parser.Parse("\"TEST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TEST");

            result = parser.Parse("\"TE\\\"ST\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TE\"ST");
        }

        [Test]
        public void StrippedSingleQuotedStringWithEscapedQuotes_Tests()
        {
            var parser = StrippedSingleQuotedString();
            var result = parser.Parse("'TEST'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("TEST");
        }
    }
}