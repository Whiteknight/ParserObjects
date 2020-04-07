using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ParserObjects.Tests.Parsers.Specialty
{
    public class IdentifierParserMethods
    {
        [Test]
        public void Parse_AllLower()
        {
            var target = ParserObjects.Parsers.Specialty.IdentifierParserMethods.CamelCase();
            var result = target.Parse("test").Value.ToList();
            result.Should().ContainInOrder("test");
        }

        [Test]
        public void Parse_Camel()
        {
            var target = ParserObjects.Parsers.Specialty.IdentifierParserMethods.CamelCase();
            var result = target.Parse("ThisIsATest").Value.ToList();
            result.Should().ContainInOrder("This", "Is", "A",  "Test");
        }

        [Test]
        public void Parse_StartsLower()
        {
            var target = ParserObjects.Parsers.Specialty.IdentifierParserMethods.CamelCase();
            var result = target.Parse("thisIsATest").Value.ToList();
            result.Should().ContainInOrder("this", "Is", "A", "Test");
        }

        [Test]
        public void Parse_CamelAcronym()
        {
            var target = ParserObjects.Parsers.Specialty.IdentifierParserMethods.CamelCase();
            var result = target.Parse("ThisIsABCTest").Value.ToList();
            result.Should().ContainInOrder("This", "Is", "ABC", "Test");
        }

        [Test]
        public void Parse_CamelStartsAcronym()
        {
            var target = ParserObjects.Parsers.Specialty.IdentifierParserMethods.CamelCase();
            var result = target.Parse("ABCThisIsTest").Value.ToList();
            result.Should().ContainInOrder("ABC", "This", "Is", "Test");
        }

        [Test]
        public void Parse_StartsNumber()
        {
            var target = ParserObjects.Parsers.Specialty.IdentifierParserMethods.CamelCase();
            var result = target.Parse("123ThisIsTest").Value.ToList();
            result.Should().ContainInOrder("123", "This", "Is", "Test");
        }
    }
}
