using static ParserObjects.Parsers.JS;

namespace ParserObjects.Tests.JavaScript;

public static class StringTests
{
    public class SingleQuotes
    {
        [Test]
        public void Parse_Tests()
        {
            var parser = String();
            var result = parser.Parse("'abcd'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'abcd'");
        }

        [Test]
        public void Parse_Escapes()
        {
            var parser = String();
            var result = parser.Parse("'\\f\\n\\r\\x0A'");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("'\\f\\n\\r\\x0A'");
        }

        [Test]
        public void Parse_InvalidEscapes()
        {
            var parser = String();
            var result = parser.Parse("'\\z'");
            result.Success.Should().BeFalse();
        }
    }

    public class DoubleQuotes
    {
        [Test]
        public void Parse_Tests()
        {
            var parser = String();
            var result = parser.Parse("\"abcd\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"abcd\"");
        }

        [Test]
        public void Parse_Escapes()
        {
            var parser = String();
            var result = parser.Parse("\"\\f\\n\\r\\x0A\"");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("\"\\f\\n\\r\\x0A\"");
        }

        [Test]
        public void Parse_InvalidEscapes()
        {
            var parser = String();
            var result = parser.Parse("\"\\z\"");
            result.Success.Should().BeFalse();
        }
    }
}
