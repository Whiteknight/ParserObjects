using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public static class MatchCharTests
{
    public class LiteralCaseSensitive
    {
        [Test]
        public void Success()
        {
            var target = MatchChar('a');
            var result = target.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Failure()
        {
            var target = MatchChar('a');
            var result = target.Parse("x");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Empty()
        {
            var target = MatchChar('a');
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void EndSentinel()
        {
            var target = MatchChar('\0');
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }

    public class LiteralCaseInsensitive
    {
        [TestCase('a', "a")]
        [TestCase('a', "A")]
        [TestCase('A', "a")]
        [TestCase('A', "A")]
        public void Success(char pattern, string input)
        {
            var target = MatchChar(pattern, caseInsensitive: true);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be(input[0]);
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Failure()
        {
            var target = MatchChar('a', caseInsensitive: true);
            var result = target.Parse("x");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Empty()
        {
            var target = MatchChar('a', caseInsensitive: true);
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void EndSentinel()
        {
            var target = MatchChar('\0', caseInsensitive: true);
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }

    public class Predicate
    {
        [Test]
        public void Success()
        {
            var target = MatchChar(c => c != 'X');
            var result = target.Parse("a");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
        }

        [Test]
        public void Failure()
        {
            var target = MatchChar(c => c != 'X');
            var result = target.Parse("X");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void Empty()
        {
            var target = MatchChar(c => c != 'X');
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }

        [Test]
        public void EndSentinel()
        {
            var target = MatchChar(c => c != 'X');
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
        }
    }
}
