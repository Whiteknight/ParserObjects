﻿using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ListParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = List(Any(), false);
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(3);
            var list = result.Value.ToList();
            list[0].Should().Be('a');
            list[1].Should().Be('b');
            list[2].Should().Be('c');
        }

        [Test]
        public void Parse_None()
        {
            var parser = List(Match(char.IsNumber), false);
            var input = new StringCharacterSequence("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count().Should().Be(0);
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any();
            var parser = List(anyParser, false);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }
    }
}
