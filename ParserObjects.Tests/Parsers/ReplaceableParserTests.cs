﻿using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class ReplaceableParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Parse_Fail()
        {
            var anyParser = Fail<char>();
            var target = Replaceable(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void SetParser_Test()
        {
            var anyParser = Any();
            var failParser = Fail<char>();
            var target = new ReplaceableParser<char, char>(failParser);
            target.SetParser(anyParser);
            var input = new StringCharacterSequence("abc");
            var result = target.Parse(input);
            result.Value.Should().Be('a');
        }

        [Test]
        public void GetChildren_Test()
        {
            var anyParser = Any();
            var target = Replaceable(anyParser);
            var result = target.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(anyParser);
        }
    }
}
