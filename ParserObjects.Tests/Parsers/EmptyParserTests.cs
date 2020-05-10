using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class EmptyParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Empty();
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(null);
        }

        [Test]
        public void ParseUnTyped_Test()
        {
            var parser = Empty();
            var input = new StringCharacterSequence("abc");
            var result = parser.ParseUntyped(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(null);
        }

        [Test]
        public void Parse_End()
        {
            var parser = Empty();
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(null);
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Empty();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
