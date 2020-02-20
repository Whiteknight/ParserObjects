using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class EmptyParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = Empty<char>();
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(null);
        }

        [Test]
        public void ParseUnTyped_Test()
        {
            var parser = Empty<char>();
            var input = new StringCharacterSequence("abc");
            var result = parser.ParseUntyped(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(null);
        }

        [Test]
        public void Parse_End()
        {
            var parser = Empty<char>();
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(null);
        }

        [Test]
        public void GetChildren_Test()
        {
            var parser = Empty<char>();
            parser.GetChildren().Count().Should().Be(0);
        }
    }
}
