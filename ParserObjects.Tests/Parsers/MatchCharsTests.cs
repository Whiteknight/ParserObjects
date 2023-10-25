using System.Linq;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public static class MatchCharsTests
{
    public class CaseSensitive
    {
        [Test]
        public void ParseSuccess()
        {
            var target = MatchChars("test");
            var result = target.Parse("test");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("test");
        }

        [Test]
        public void ParseFail()
        {
            var target = MatchChars("test");
            var result = target.Parse("FAIL");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void MatchSuccess()
        {
            var target = MatchChars("test");
            var result = target.Match("test");
            result.Should().BeTrue();
        }

        [Test]
        public void MatchFail()
        {
            var target = MatchChars("test");
            var result = target.Match("FAIL");
            result.Should().BeFalse();
        }

        [Test]
        public void ToBnf()
        {
            var target = MatchChars("test");
            var bnf = target.ToBnf();
            bnf.Should().Contain(":= 't' 'e' 's' 't'");
        }

        [Test]
        public void Named()
        {
            var target = MatchChars("test").Named("test");
            target.Name.Should().Be("test");
        }

        [Test]
        public void GetChildren()
        {
            var target = MatchChars("test");
            var children = target.GetChildren();
            children.Count().Should().Be(0);
        }
    }

    public class CaseInsensitive
    {
        [Test]
        public void ParseSuccess()
        {
            var target = MatchChars("TEST", true);
            var result = target.Parse("test");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("test");
        }

        [Test]
        public void ParseFail()
        {
            var target = MatchChars("test", true);
            var result = target.Parse("FAIL");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void MatchSuccess_WrongCase()
        {
            var target = MatchChars("TEST", true);
            var result = target.Match("test");
            result.Should().BeTrue();
        }

        [Test]
        public void MatchSuccess_SameCase()
        {
            var target = MatchChars("test", true);
            var result = target.Match("test");
            result.Should().BeTrue();
        }

        [Test]
        public void MatchFail()
        {
            var target = MatchChars("test");
            var result = target.Match("FAIL");
            result.Should().BeFalse();
        }
    }
}
