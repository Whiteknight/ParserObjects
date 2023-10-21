﻿using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class MatchAnyTests
{
    public class CaseSensitive
    {
        [Test]
        public void Parse_Operators()
        {
            var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });

            var input = FromString("===>=<=><<==");

            target.Parse(input).Value.Should().Be("==");
            target.Parse(input).Value.Should().Be("=");
            target.Parse(input).Value.Should().Be(">=");
            target.Parse(input).Value.Should().Be("<=");
            target.Parse(input).Value.Should().Be(">");
            target.Parse(input).Value.Should().Be("<");
            target.Parse(input).Value.Should().Be("<=");
            target.Parse(input).Value.Should().Be("=");
        }

        [Test]
        public void Parse_IsAtEnd()
        {
            var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });

            var input = FromString("");

            var result = target.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });
            target.GetChildren().Count().Should().Be(0);
        }

        [Test]
        public void Parse_Operators_Fail()
        {
            var target = MatchAny(new[] { "=", "==", ">=", "<=", "<", ">" });

            var input = FromString("X===>=<=><<==");

            target.Parse(input).Success.Should().BeFalse();
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = MatchAny(new[] { 'A', 'B', 'C' }).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := MATCH()");
        }
    }

    public class CaseInsensitive
    {
        [TestCase("A", "A")]
        [TestCase("a", "A")]
        [TestCase("AB", "AB")]
        [TestCase("aB", "AB")]
        [TestCase("Ab", "AB")]
        [TestCase("AABB", "aabb")]
        [TestCase("aaBB", "aabb")]
        [TestCase("AAbb", "aabb")]
        [TestCase("AaAbBb", "AaabbB")]
        public void Parse(string input, string expected)
        {
            var target = MatchAny(new[] { "A", "AB", "aabb", "AaabbB" }, caseInsensitive: true);

            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(expected);
        }
    }
}
