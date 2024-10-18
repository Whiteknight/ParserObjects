using System.Collections.Generic;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public static class NotMatchAnyTests
{
    public class Chars
    {
        [Test]
        public void Success()
        {
            var target = NotMatchAny(new[] { 'a', 'b', 'c' });
            var result = target.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');
        }

        [Test]
        public void Fail()
        {
            var target = NotMatchAny(new[] { 'a', 'b', 'c' });
            var result = target.Parse("a");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void EmptyList()
        {
            var target = NotMatchAny(Array.Empty<char>());
            var result = target.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');
        }

        [Test]
        public void Null()
        {
            var target = NotMatchAny((ICollection<char>)null);
            var result = target.Parse("X");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('X');
        }
    }

    public class StringCaseSensitive
    {
        [TestCase("aBc", "a", false)]
        [TestCase("aBc", "b", true)]
        [TestCase("aBc", "c", false)]
        [TestCase("aBc", "X", true)]
        public void Test(string possibilities, string input, bool shouldMatch)
        {
            var target = NotMatchAny(possibilities);
            var result = target.Parse(input);
            result.Success.Should().Be(shouldMatch);
            if (shouldMatch)
                result.Value.Should().Be(input[0]);
        }

        [Test]
        public void NoPossibilities()
        {
            var target = NotMatchAny("");
            var result = target.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
        }
    }

    public class StringCaseInsensitive
    {
        [TestCase("aBc", "a", false)]
        [TestCase("aBc", "b", false)]
        [TestCase("aBc", "C", false)]
        [TestCase("aBc", "X", true)]
        public void Test(string possibilities, string input, bool shouldMatch)
        {
            var target = NotMatchAny(possibilities, caseInsensitive: true);
            var result = target.Parse(input);
            result.Success.Should().Be(shouldMatch);
            if (shouldMatch)
                (result.Value.Equals(char.ToLower(input[0])) || result.Value.Equals(char.ToUpper(input[0]))).Should().BeTrue();
        }
    }
}
