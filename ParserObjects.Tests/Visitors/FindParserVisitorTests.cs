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
            result.Should().BeSameAs(needle);
        }

        [Test]
        public void FindNamed_Extension_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = haystack.FindNamed("needle");
            result.Should().BeSameAs(needle);
        }

        [Test]
        public void OfType_Test()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.OfType<FailParser<char, char>>(haystack);
            result.Count.Should().Be(1);
            result[0].Should().BeSameAs(needle);
        }

        [Test]
        public void Replace_Fail_RootNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(null, r => false, needle);
            result.IsSuccess.Should().BeFalse();
        }

        [Test]
        public void Replace_Fail_PredicateNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(haystack, (Func<IReplaceableParserUntyped, bool>)null, needle);
            result.IsSuccess.Should().BeFalse();
        }

        [Test]
        public void Replace_Fail_ReplacementNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace(haystack, r => false, null);
            result.IsSuccess.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_RootNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(null, r => false, old => needle);
            result.IsSuccess.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_PredicateNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(haystack, (Func<IReplaceableParserUntyped, bool>)null, old => needle);
            result.IsSuccess.Should().BeFalse();
        }

        [Test]
        public void Replace_Transform_Fail_ReplacementNull()
        {
            var needle = Fail<char>().Named("needle");
            var haystack = (Any(), Any(), Any(), needle).First();
            var result = FindParserVisitor.Replace<char, char>(haystack, r => false, null);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
