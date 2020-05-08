﻿using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Sequences;
using static ParserObjects.Parsers.ParserMethods;
using static ParserObjects.Parsers.Specialty.CStyleParserMethods;

namespace ParserObjects.Tests.Parsers
{
    public class SeparatedListParserTests
    {
        [Test]
        public void Parse_Test()
        {
            var parser = SeparatedList(
                Integer(),
                Match<char>(","),
                atLeastOne: false
            );
            var input = new StringCharacterSequence("1,2,3,4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
            value[3].Should().Be(4);
        }

        [Test]
        public void ListSeparatedBy_Parse_Test()
        {
            var parser = Integer().ListSeparatedBy(
                Match<char>(","),
                atLeastOne: false
            );
            var input = new StringCharacterSequence("1,2,3,4");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
            value[3].Should().Be(4);
        }

        [Test]
        public void Parse_Empty()
        {
            var parser = SeparatedList(
                Integer(),
                Match<char>(","),
                atLeastOne: false
            );
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Count().Should().Be(0);
        }

        [Test]
        public void Parse_AtLeastOne_HasOne()
        {
            var parser = SeparatedList(
                Integer(),
                Match<char>(","),
                atLeastOne: true
            );
            var input = new StringCharacterSequence("1");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
        }

        [Test]
        public void Parse_AtLeastOne_Multiple()
        {
            var parser = SeparatedList(
                Integer(),
                Match<char>(","),
                atLeastOne: true
            );
            var input = new StringCharacterSequence("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value[0].Should().Be(1);
            value[1].Should().Be(2);
            value[2].Should().Be(3);
        }

        [Test]
        public void Parse_AtLeastOne_Empty()
        {
            var parser = SeparatedList(
                Integer(),
                Match<char>(","),
                atLeastOne: true
            );
            var input = new StringCharacterSequence("");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Minimum_Fail()
        {
            var parser = SeparatedList(
                Integer(),
                Match<char>(","),
                4
            );
            var input = new StringCharacterSequence("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Maximum()
        {
            var parser = SeparatedList(
                Integer(),
                Match<char>(","),
                0, 
                2
            );
            var input = new StringCharacterSequence("1,2,3");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            var value = result.Value.ToList();
            value.Count.Should().Be(2);
            value[0].Should().Be(1);
            value[1].Should().Be(2);
        }
    }
}
