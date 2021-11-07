using System;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Parsers;
using ParserObjects.Visitors;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Visitors
{
    public class FindParserVisitorTests
    {
        [Test]
        public void Named_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Named("needle", haystack);
            result.Success.Should().BeTrue();
            result.Value.Should().BeSameAs(needle);
        }

        [Test]
        public void FindNamed_Extension_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = haystack.FindNamed("needle");
            result.Success.Should().BeTrue();
            result.Value.Should().BeSameAs(needle);
        }

        [Test]
        public void OfType_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.OfType<Fail<char, char>.Parser>(haystack);
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(needle);
        }

        [Test]
        public void Replace_Fail_RootNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(null, _ => false, needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Fail_PredicateNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(haystack, (Func<IReplaceableParserUntyped, bool>)null, needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Fail_ReplacementNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(haystack, r => false, null);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_RootNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(null, _ => false, _ => needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_PredicateNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(haystack, (Func<IReplaceableParserUntyped, bool>)null, _ => needle);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_ReplacementNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(haystack, _ => false, null);
            result.Success.Should().BeFalse();
        }
    }
}
