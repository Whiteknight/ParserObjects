using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class MatchAnyTests
{
    public class StringsCaseSensitive
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
        public void EmptyPossibilities()
        {
            var target = MatchAny(new string[0]);
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void NullPossibilities()
        {
            var target = MatchAny((IEnumerable<string>)null);
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = MatchAny(new[] { 'A', 'B', 'C' }).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := MATCH()");
        }
    }

    public class StringsCaseInsensitive
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

    public class Chars
    {
        [Test]
        public void Success()
        {
            var target = MatchAny(new[] { 'a', 'b', 'c' });
            var result = target.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }

        [Test]
        public void Fail()
        {
            var target = MatchAny(new[] { 'a', 'b', 'c' });
            var result = target.Parse("X");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void EmptyList()
        {
            var target = MatchAny(Array.Empty<char>());
            var result = target.Parse("X");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Null()
        {
            var target = MatchAny((ICollection<char>)null);
            var result = target.Parse("X");
            result.Success.Should().BeFalse();
        }
    }

    public class StringCaseSensitive
    {
        [TestCase("aBc", "a", true)]
        [TestCase("aBc", "b", false)]
        [TestCase("aBc", "c", true)]
        public void Test(string possibilities, string input, bool shouldMatch)
        {
            var target = MatchAny(possibilities);
            var result = target.Parse(input);
            result.Success.Should().Be(shouldMatch);
            if (shouldMatch)
                result.Value.Should().Be(input[0]);
        }

        [Test]
        public void NoPossibilities()
        {
            var target = MatchAny("");
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
        }
    }

    public class StringCaseInsensitive
    {
        [TestCase("aBc", "a", true)]
        [TestCase("aBc", "b", true)]
        [TestCase("aBc", "C", true)]
        public void Test(string possibilities, string input, bool shouldMatch)
        {
            var target = MatchAny(possibilities, caseInsensitive: true);
            var result = target.Parse(input);
            result.Success.Should().Be(shouldMatch);
            if (shouldMatch)
                (result.Value.Equals(char.ToLower(input[0])) || result.Value.Equals(char.ToUpper(input[0]))).Should().BeTrue();
        }
    }
}
