﻿using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class NegativeLookaheadParserTests
    {
        [Test]
        public void NotFollowedBy_Success()
        {
            var parser = Match('[').NotFollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[test]");
            parser.Parse(input).Value.Should().Be('[');
        }

        [Test]
        public void NotFollowedBy_Fail()
        {
            var parser = Match('[').NotFollowedBy(Match<char>("~"));
            var input = new StringCharacterSequence("[~test]");
            parser.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail()
        {
            var failParser = Fail<char, char>();
            var parser = NegativeLookahead(failParser);

            var input = new StringCharacterSequence("abc");
            parser.Parse(input).Success.Should().Be(true);
            parser.Parse(input).Value.Should().Be(true);
        }

        [Test]
        public void ReplaceChild_Test()
        {
            var failParser = Fail<char, char>();
            var anyParser = Any<char>();
            var parser = NegativeLookahead(failParser);
            parser = parser.ReplaceChild(failParser, anyParser) as IParser<char, bool>;

            var input = new StringCharacterSequence("abc");
            parser.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void ReplaceChild_Same()
        {
            var failParser = Fail<char, char>();
            var parser = NegativeLookahead(failParser);
            var result = parser.ReplaceChild(null, null) as IParser<char, bool>;

            result.Should().BeSameAs(parser);
        }

        [Test]
        public void GetChildren_Test()
        {
            var failParser = Fail<char, char>();
            var parser = NegativeLookahead(failParser);
            var result = parser.GetChildren().ToList();
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(failParser);
        }
    }
}