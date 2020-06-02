using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.Parsers.Specialty.IdentifierParserMethods;

namespace ParserObjects.Tests.Parsers.Specialty
{
    public class IdentifierParserMethods
    {
        [Test]
        public void CamelCase_AllLower()
        {
            var target = CamelCase();
            var result = target.Parse("test").Value.ToList();
            result.Should().ContainInOrder("test");
        }

        [Test]
        public void CamelCase_Test()
        {
            var target = CamelCase();
            var result = target.Parse("ThisIsATest").Value.ToList();
            result.Should().ContainInOrder("This", "Is", "A",  "Test");
        }

        [Test]
        public void CamelCase_StartsLower()
        {
            var target = CamelCase();
            var result = target.Parse("thisIsATest").Value.ToList();
            result.Should().ContainInOrder("this", "Is", "A", "Test");
        }

        [Test]
        public void CamelCase_ContainsAcronym()
        {
            var target = CamelCase();
            var result = target.Parse("ThisIsABCTest").Value.ToList();
            result.Should().ContainInOrder("This", "Is", "ABC", "Test");
        }

        [Test]
        public void CamelCase_StartsAcronym()
        {
            var target = CamelCase();
            var result = target.Parse("ABCThisIsTest").Value.ToList();
            result.Should().ContainInOrder("ABC", "This", "Is", "Test");
        }

        [Test]
        public void CamelCase_Fail_StartsNumber()
        {
            var target = CamelCase();
            var result = target.Parse("123ThisIsTest");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void LowerCamelCase_AllLower()
        {
            var target = LowerCamelCase();
            var result = target.Parse("test").Value.ToList();
            result.Should().ContainInOrder("test");
        }

        [Test]
        public void LowerCamelCase_Fail()
        {
            var target = LowerCamelCase();
            var result = target.Parse("ThisIsATest");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void LowerCamelCase_StartsLower()
        {
            var target = LowerCamelCase();
            var result = target.Parse("thisIsATest").Value.ToList();
            result.Should().ContainInOrder("this", "Is", "A", "Test");
        }

        [Test]
        public void LowerCamelCase_ContainsAcronym()
        {
            var target = LowerCamelCase();
            var result = target.Parse("thisIsABCTest").Value.ToList();
            result.Should().ContainInOrder("this", "Is", "ABC", "Test");
        }

        [Test]
        public void LowerCamelCase_StartsAcronym()
        {
            var target = LowerCamelCase();
            var result = target.Parse("ABCThisIsTest");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void LowerCamelCase_Fail_StartsNumber()
        {
            var target = LowerCamelCase();
            var result = target.Parse("123ThisIsTest");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void UpperCamelCase_AllLower()
        {
            var target = UpperCamelCase();
            var result = target.Parse("test");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void UpperCamelCase_Test()
        {
            var target = UpperCamelCase();
            var result = target.Parse("ThisIsATest").Value.ToList();
            result.Should().ContainInOrder("This", "Is", "A", "Test");
        }

        [Test]
        public void UpperCamelCase_StartsLower()
        {
            var target = UpperCamelCase();
            var result = target.Parse("thisIsATest");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void UpperCamelCase_ContainsAcronym()
        {
            var target = UpperCamelCase();
            var result = target.Parse("ThisIsABCTest").Value.ToList();
            result.Should().ContainInOrder("This", "Is", "ABC", "Test");
        }

        [Test]
        public void UpperCamelCase_StartsAcronym()
        {
            var target = UpperCamelCase();
            var result = target.Parse("ABCThisIsTest").Value.ToList();
            result.Should().ContainInOrder("ABC", "This", "Is", "Test");
        }

        [Test]
        public void UpperCamelCase_Fail_StartsNumber()
        {
            var target = UpperCamelCase();
            var result = target.Parse("123ThisIsTest");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void SpinalCase_SingleWord()
        {
            var target = SpinalCase();
            var result = target.Parse("test").Value.ToList();
            result.Should().ContainInOrder("test");
        }

        [Test]
        public void SpinalCase_Test()
        {
            var target = SpinalCase();
            var result = target.Parse("this-is-a-test").Value.ToList();
            result.Should().ContainInOrder("this", "is", "a", "test");
        }

        [Test]
        public void ScreamingSpinalCase_SingleWord()
        {
            var target = ScreamingSpinalCase();
            var result = target.Parse("TEST").Value.ToList();
            result.Should().ContainInOrder("TEST");
        }

        [Test]
        public void ScreamingSpinalCase_Test()
        {
            var target = ScreamingSpinalCase();
            var result = target.Parse("THIS-IS-A-TEST").Value.ToList();
            result.Should().ContainInOrder("THIS", "IS", "A", "TEST");
        }

        [Test]
        public void SnakeCase_SingleWord()
        {
            var target = SnakeCase();
            var result = target.Parse("test").Value.ToList();
            result.Should().ContainInOrder("test");
        }

        [Test]
        public void SnakeCase_Test()
        {
            var target = SnakeCase();
            var result = target.Parse("this_is_a_test").Value.ToList();
            result.Should().ContainInOrder("this", "is", "a", "test");
        }

        [Test]
        public void ScreamingSnakeCase_SingleWord()
        {
            var target = ScreamingSnakeCase();
            var result = target.Parse("TEST").Value.ToList();
            result.Should().ContainInOrder("TEST");
        }

        [Test]
        public void ScreamingSnakeCase_Test()
        {
            var target = ScreamingSnakeCase();
            var result = target.Parse("THIS_IS_A_TEST").Value.ToList();
            result.Should().ContainInOrder("THIS", "IS", "A", "TEST");
        }
    }
}
