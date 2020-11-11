﻿using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.CStyleParserMethods;

namespace ParserObjects.Tests
{
    public class CStyleParserMethods
    {
        [Test]
        public void CStyleIntegerLiteral_Tests()
        {
            var parser = Integer();
            var result = parser.Parse(new StringCharacterSequence("1234"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(1234);
        }

        [Test]
        public void CStyleHexadecimalLiteral_Tests()
        {
            var parser = HexadecimalInteger();
            var result = parser.Parse(new StringCharacterSequence("0xAB12"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(0xAB12);
        }

        [Test]
        public void CStyleDoubleLiteral_Tests()
        {
            var parser = Double();
            var result = parser.Parse(new StringCharacterSequence("12.34"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be(12.34);
        }

        [Test]
        public void CStyleCommentLiteral_Tests()
        {
            var parser = Comment();
            var result = parser.Parse(new StringCharacterSequence("/* TEST */"));
            result.Success.Should().BeTrue();
            result.Value.Should().Be("/* TEST */");
        }

        [Test]
        public void CStyleIdentifier_Tests()
        {
            var parser = Identifier();
            parser.CanMatch("_").Should().BeTrue();
            parser.CanMatch("a").Should().BeTrue();
            parser.CanMatch("a1").Should().BeTrue();
            parser.CanMatch("_1").Should().BeTrue();
            parser.CanMatch("test").Should().BeTrue();
            parser.CanMatch("0").Should().BeFalse();
            parser.CanMatch("-a").Should().BeFalse();
        }
    }
}