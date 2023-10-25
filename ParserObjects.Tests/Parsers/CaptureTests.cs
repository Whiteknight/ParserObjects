using static ParserObjects.Parsers<char>;
using static ParserObjects.Parsers;

namespace ParserObjects.Tests.Parsers;

public static class CaptureTests
{
    public class Inputs
    {
        [Test]
        public void Parse_Test()
        {
            var target = Capture(
                MatchChars("abc"),
                Any(),
                Any(),
                Any(),
                MatchChars("ghi")
            ).Transform(c => new string(c));
            var result = target.Parse("abcdefghi");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcdefghi");
        }

        [Test]
        public void Parse_Empty()
        {
            var target = Capture().Transform(c => new string(c));
            var result = target.Parse("abcdefghi");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("");
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Capture(Any()).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := (.)");
        }
    }

    public class CharString
    {
        [Test]
        public void Parse_Test()
        {
            var target = CaptureString(
                MatchChars("abc"),
                Any(),
                Any(),
                Any(),
                MatchChars("ghi")
            );
            var result = target.Parse("abcdefghi");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("abcdefghi");
        }

        [Test]
        public void Parse_Empty()
        {
            var target = CaptureString();
            var result = target.Parse("abcdefghi");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("");
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Capture(Any()).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := (.)");
        }
    }
}
