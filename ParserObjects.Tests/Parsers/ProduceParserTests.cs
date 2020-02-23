﻿using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class ProduceParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var target = Produce<char, int>(x => 5);
            var input = new StringCharacterSequence("abc");
            target.Parse(input).Value.Should().Be(5);
        }

        [Test]
        public void ParseUntyped_Test()
        {
            var target = Produce<char, int>(x => 5);
            var input = new StringCharacterSequence("abc");
            target.ParseUntyped(input).Value.Should().Be(5);
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = Produce<char, int>(x => 5);
            target.GetChildren().Count().Should().Be(0);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var target = Produce<char, int>(x => 5);
            var input = new StringCharacterSequence("abc");
            target.ReplaceChild(null, null).Should().Be(target);
        }
    }
}