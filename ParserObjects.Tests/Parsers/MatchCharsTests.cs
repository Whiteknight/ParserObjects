using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers;

public static class MatchCharsTests
{
    public class CaseSensitive
    {
        [Test]
        public void Parse_Success()
        {
            var target = MatchChars("test");
            var result = target.Parse("test");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("test");
        }

        [Test]
        public void Parse_Success_NonCharSequence()
        {
            var target = MatchChars("test");
            var input = FromEnumerable(new[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("test");
        }

        [Test]
        public void Parse_Success_NonCharSequence_Length1()
        {
            var target = MatchChars("t");
            var input = FromEnumerable(new[] { (byte)'t' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("t");
        }

        [Test]
        public void Parse_Fail()
        {
            var target = MatchChars("test");
            var result = target.Parse("FAIL");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_NonCharSequence()
        {
            var target = MatchChars("test");
            var input = FromEnumerable(new[] { (byte)'t', (byte)'e', (byte)'X', (byte)'t' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_NonCharSequence_Length1()
        {
            var target = MatchChars("t");
            var input = FromEnumerable(new[] { (byte)'X' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Match_Success()
        {
            var target = MatchChars("test");
            var result = target.Match("test");
            result.Should().BeTrue();
        }

        [Test]
        public void Match_Empty()
        {
            var target = MatchChars("");
            var result = target.Match("test");
            result.Should().BeTrue();
        }

        [Test]
        public void Match_Fail()
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
        public void Parse_Success()
        {
            var target = MatchChars("TEST", true);
            var result = target.Parse("test");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("test");
        }

        [Test]
        public void Parse_Success_NonCharSequence()
        {
            var target = MatchChars("test", true);
            var input = FromEnumerable(new[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("test");
        }

        [Test]
        public void Parse_Success_NonCharSequence_Length1()
        {
            var target = MatchChars("t", true);
            var input = FromEnumerable(new[] { (byte)'t' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeTrue();
            result.Value.Should().Be("t");
        }

        [Test]
        public void Parse_Fail()
        {
            var target = MatchChars("test", true);
            var result = target.Parse("FAIL");
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_NonCharSequence()
        {
            var target = MatchChars("test", true);
            var input = FromEnumerable(new[] { (byte)'t', (byte)'e', (byte)'X', (byte)'t' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail_NonCharSequence_Length1()
        {
            var target = MatchChars("t", true);
            var input = FromEnumerable(new[] { (byte)'X' }).Select(b => (char)b);
            var result = target.Parse(input);
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Match_Success_WrongCase()
        {
            var target = MatchChars("TEST", true);
            var result = target.Match("test");
            result.Should().BeTrue();
        }

        [Test]
        public void Match_Success_SameCase()
        {
            var target = MatchChars("test", true);
            var result = target.Match("test");
            result.Should().BeTrue();
        }

        [Test]
        public void Match_Fail()
        {
            var target = MatchChars("test", true);
            var result = target.Match("FAIL");
            result.Should().BeFalse();
        }
    }
}
