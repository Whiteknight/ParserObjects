using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.Specialty.LineParserMethods;

namespace ParserObjects.Tests.Parsers.Specialty
{
    public class LineParserMethodsTests
    {
        [Test]
        public void PrefixedLine_NoPrefix()
        {
            var parser = PrefixedLine("");
            var input = new StringCharacterSequence(@"line
NOT LINE");
            parser.Parse(input).Value.Should().Be("line");
        }

        [Test]
        public void PrefixedLine_Prefix()
        {
            var parser = PrefixedLine("XXX");
            var input = new StringCharacterSequence(@"XXXline
NOT LINE");
            parser.Parse(input).Value.Should().Be("XXXline");
        }

        [Test]
        public void PrefixedLine_Prefix_NoMatch()
        {
            var parser = PrefixedLine("YYY");
            var input = new StringCharacterSequence(@"XXXline
NOT LINE");
            parser.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void Line_Test()
        {
            var parser = Line();
            var input = new StringCharacterSequence(@"line
NOT LINE");
            parser.Parse(input).Value.Should().Be("line");
        }
    }
}
